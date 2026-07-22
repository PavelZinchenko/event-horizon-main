using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Constructor.Ships;
using GameDatabase;
using GameServices.Player;
using Services.Resources;
using UnityEngine;
using Zenject;

namespace GameServices.Multiplayer
{
    /// <summary>
    /// Two-player TCP session used through a SakuraFRP TCP tunnel. SakuraFRP only
    /// forwards the socket; the host always listens on local port 8779.
    /// </summary>
    public sealed class MultiplayerSession : IInitializable, ITickable, IDisposable
    {
        public const int HostPort = 8779;
        public static MultiplayerSession Instance { get; private set; }

        public MultiplayerSession(IDatabase database, PlayerFleet playerFleet)
        {
            _database = database;
            _playerFleet = playerFleet;
        }

        public bool IsActive => _client != null && _client.Connected;
        public bool IsConnecting { get; private set; }
        public bool IsHost { get; private set; }
        public bool IsWaitingForGuest => IsHost && _listener != null && _client == null;
        public bool IsInLobby => IsActive && _resourcesPreparedSent && _peerResourcesPrepared;
        public bool IsReady => _localReady;
        public IReadOnlyList<IShip> RemoteFleet => _remoteFleet;
        public string Status { get; private set; } = "未连接";
        public NetInput LatestRemoteInput { get; private set; }
        public SnapshotBatch LatestSnapshot { get; private set; }

        public event Action<string> StatusChanged;
        public event Action BattleReady;

        public void Initialize() => Instance = this;

        public async void Host()
        {
            Disconnect();
            var operation = Interlocked.Increment(ref _connectionOperation);
            IsHost = true;
            IsConnecting = true;
            SetStatus("正在检查本地端口 8779…");
            TcpListener listener = null;
            try
            {
                var deadline = DateTime.UtcNow.AddMilliseconds(HostPortCheckTimeoutMilliseconds);
                while (listener == null)
                {
                    if (!IsCurrentOperation(operation))
                        return;

                    try
                    {
                        var candidate = new TcpListener(IPAddress.Any, HostPort);
                        try
                        {
                            candidate.Start(1);
                            listener = candidate;
                            _listener = listener;
                        }
                        catch
                        {
                            StopListener(candidate);
                            throw;
                        }
                    }
                    catch (SocketException)
                    {
                        if (DateTime.UtcNow >= deadline)
                            throw new TimeoutException("15 秒内无法使用本地端口 8779");

                        await Task.Delay(HostPortRetryMilliseconds);
                    }
                }

                if (!IsCurrentOperation(operation))
                {
                    StopListener(listener);
                    return;
                }

                // The port is ready now.  The host enters the lobby immediately;
                // the guest may join later through the SakuraFRP TCP address.
                IsConnecting = false;
                SetStatus("等待大厅：本地 8779 端口已就绪，等待客机连接…");
                var client = await listener.AcceptTcpClientAsync();

                // Closing the lobby or starting another connection intentionally
                // disposes the old listener.  Do not let that old await clean up
                // or report an error for the newer connection attempt.
                if (!IsCurrentOperation(operation))
                {
                    client.Close();
                    return;
                }

                if (ReferenceEquals(_listener, listener)) _listener = null;
                StopListener(listener);
                _client = client;
                StartConnection();
            }
            catch (Exception error)
            {
                if (!IsCurrentOperation(operation)) return;
                IsConnecting = false;
                if (ReferenceEquals(_listener, listener)) _listener = null;
                StopListener(listener);
                SetStatus(error is TimeoutException
                    ? "连接超时：15 秒内无法使用本地端口 8779"
                    : "主机启动失败：" + error.Message);
                DisconnectSocketOnly();
            }
        }

        public async void Connect(string address)
        {
            Disconnect();
            var operation = Interlocked.Increment(ref _connectionOperation);
            IsHost = false;
            IsConnecting = true;
            TcpClient client = null;
            try
            {
                ParseEndpoint(address, out var host, out var port);
                SetStatus("正在连接 " + host + ":" + port + "…");
                client = new TcpClient();
                _client = client;
                await client.ConnectAsync(host, port);
                if (!IsCurrentOperation(operation))
                {
                    client.Close();
                    return;
                }
                StartConnection();
            }
            catch (Exception error)
            {
                if (!IsCurrentOperation(operation)) return;
                IsConnecting = false;
                SetStatus("连接失败：" + error.Message);
                DisconnectSocketOnly();
            }
        }

        public void Tick()
        {
            while (_mainThreadActions.TryDequeue(out var action)) action();
            while (_incoming.TryDequeue(out var envelope))
            {
                try { Process(envelope); }
                catch (Exception error) { SetStatus("联机数据错误：" + error.Message); }
            }
        }

        public void SendInput(NetInput input)
        {
            if (IsActive && !IsHost) Send("input", JsonUtility.ToJson(input));
        }

        public void SendSnapshot(SnapshotBatch snapshot)
        {
            if (IsActive && IsHost) Send("snapshot", JsonUtility.ToJson(snapshot));
        }

        public void SetReady()
        {
            if (!IsInLobby || _localReady)
                return;

            _localReady = true;
            Send("ready", AppConfig.version);
            SetStatus(IsHost
                ? "等待大厅：主机已准备，等待客机准备…"
                : "等待大厅：客机已准备，等待主机开始…");
            TryStartHost();
        }

        public void Dispose()
        {
            Disconnect();
            if (ReferenceEquals(Instance, this)) Instance = null;
        }

        public void Disconnect()
        {
            Interlocked.Increment(ref _connectionOperation);
            IsConnecting = false;
            _cancel?.Cancel();
            DisconnectSocketOnly();
            while (_incoming.TryDequeue(out _)) { }
            while (_mainThreadActions.TryDequeue(out _)) { }
            _remoteFleet.Clear();
            _expectedTextures.Clear();
            _textureChunks.Clear();
            _resourcesPreparedSent = false;
            _peerResourcesPrepared = false;
            _localReady = false;
            _peerReady = false;
            _manifestReceived = false;
            LatestRemoteInput = null;
            LatestSnapshot = null;
            PlayerShipTextureOverrides.ClearRemoteSession();
            SetStatus("未连接");
        }

        private void StartConnection()
        {
            IsConnecting = false;
            _client.NoDelay = true;
            _stream = _client.GetStream();
            _cancel = new CancellationTokenSource();
            SetStatus(IsHost ? "客机已连接，正在交换舰队…" : "已连接，正在交换舰队…");
            _ = ReceiveLoop(_cancel.Token);
            SendLocalFleetAndTextures();
        }

        private void SendLocalFleetAndTextures()
        {
            var ships = _playerFleet.ActiveShipGroup.Ships.ToArray();
            if (ships.Length == 0)
                throw new InvalidOperationException("当前本地舰队为空");

            var fleet = FleetWebSerializer.SerializeFleet(ships);
            if (string.IsNullOrEmpty(fleet))
                throw new InvalidOperationException("舰队配置过大，无法序列化");
            Send("fleet", fleet);

            var textures = new List<TextureManifestEntry>();
            foreach (var ship in ships.GroupBy(item => item.Model.Id.Value).Select(group => group.First()))
            {
                if (!PlayerShipTextureOverrides.TryGetOverrideBytes(ship.Model.Id.Value, out var bytes)) continue;
                textures.Add(new TextureManifestEntry
                {
                    shipId = ship.Model.Id.Value,
                    hash = Hash(bytes),
                    length = bytes.Length,
                });
            }
            Send("textureManifest", JsonUtility.ToJson(new TextureManifest { entries = textures.ToArray() }));
            foreach (var entry in textures)
            {
                PlayerShipTextureOverrides.TryGetOverrideBytes(entry.shipId, out var bytes);
                var total = Math.Max(1, (bytes.Length + TextureChunkSize - 1) / TextureChunkSize);
                for (var index = 0; index < total; index++)
                {
                    var length = Math.Min(TextureChunkSize, bytes.Length - index * TextureChunkSize);
                    var chunk = new byte[length];
                    Buffer.BlockCopy(bytes, index * TextureChunkSize, chunk, 0, length);
                    Send("textureChunk", JsonUtility.ToJson(new TextureChunk
                    {
                        shipId = entry.shipId, hash = entry.hash, index = index,
                        total = total, data = Convert.ToBase64String(chunk),
                    }));
                }
            }
        }

        private void Process(Envelope message)
        {
            switch (message.type)
            {
                case "fleet":
                    _remoteFleet = FleetWebSerializer.DeserializeFleet(_database, message.data).ToList();
                    SetStatus("已收到对方舰队，正在同步自定义贴图…");
                    TryPrepare();
                    break;
                case "textureManifest":
                    var manifest = JsonUtility.FromJson<TextureManifest>(message.data) ?? new TextureManifest();
                    _manifestReceived = true;
                    _expectedTextures.Clear();
                    foreach (var item in manifest.entries ?? Array.Empty<TextureManifestEntry>())
                        _expectedTextures[item.shipId] = item;
                    TryPrepare();
                    break;
                case "textureChunk":
                    ReceiveTextureChunk(JsonUtility.FromJson<TextureChunk>(message.data));
                    TryPrepare();
                    break;
                case "resourcesPrepared":
                    _peerResourcesPrepared = true;
                    TryPrepare();
                    break;
                case "ready":
                    _peerReady = true;
                    TryStartHost();
                    break;
                case "start":
                    SetStatus("同步完成，正在进入联机战斗…");
                    BattleReady?.Invoke();
                    break;
                case "input": LatestRemoteInput = JsonUtility.FromJson<NetInput>(message.data); break;
                case "snapshot": LatestSnapshot = JsonUtility.FromJson<SnapshotBatch>(message.data); break;
                case "disconnect": SetStatus("对方已断开连接"); break;
            }
        }

        private void ReceiveTextureChunk(TextureChunk chunk)
        {
            if (chunk == null || !_expectedTextures.TryGetValue(chunk.shipId, out var expected) || expected.hash != chunk.hash)
                return;
            if (!_textureChunks.TryGetValue(chunk.shipId, out var parts))
                _textureChunks[chunk.shipId] = parts = new byte[chunk.total][];
            if (chunk.index < 0 || chunk.index >= parts.Length) return;
            parts[chunk.index] = Convert.FromBase64String(chunk.data);
            if (parts.Any(item => item == null)) return;

            var bytes = parts.SelectMany(item => item).ToArray();
            if (bytes.Length != expected.length || Hash(bytes) != expected.hash)
                throw new InvalidDataException("舰船贴图校验失败");
            PlayerShipTextureOverrides.SetRemoteOverride(chunk.shipId, bytes);
            _textureChunks.Remove(chunk.shipId);
            _expectedTextures.Remove(chunk.shipId);
        }

        private void TryPrepare()
        {
            if (!_resourcesPreparedSent && _remoteFleet.Count > 0 && _manifestReceived && _expectedTextures.Count == 0)
            {
                _resourcesPreparedSent = true;
                Send("resourcesPrepared", AppConfig.version);
            }

            if (!_resourcesPreparedSent || !_peerResourcesPrepared)
                return;

            if (!_localReady)
                SetStatus("等待大厅：资源已就绪，请点击准备");
            TryStartHost();
        }

        private void TryStartHost()
        {
            if (!IsHost || !_localReady || !_peerReady) return;
            _peerReady = false;
            Send("start", AppConfig.version);
            SetStatus("同步完成，正在进入联机战斗…");
            BattleReady?.Invoke();
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            try
            {
                var header = new byte[4];
                while (!token.IsCancellationRequested)
                {
                    await ReadExactly(header, token);
                    var length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 0));
                    if (length <= 0 || length > MaxMessageSize) throw new InvalidDataException("非法网络消息长度");
                    var data = new byte[length];
                    await ReadExactly(data, token);
                    _incoming.Enqueue(JsonUtility.FromJson<Envelope>(Encoding.UTF8.GetString(data)));
                }
            }
            catch (Exception error)
            {
                if (!token.IsCancellationRequested)
                    _mainThreadActions.Enqueue(() => SetStatus("连接已断开：" + error.Message));
            }
        }

        private async Task ReadExactly(byte[] data, CancellationToken token)
        {
            var offset = 0;
            while (offset < data.Length)
            {
                var read = await _stream.ReadAsync(data, offset, data.Length - offset, token);
                if (read == 0) throw new EndOfStreamException();
                offset += read;
            }
        }

        private async void Send(string type, string data)
        {
            if (_stream == null) return;
            var payload = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Envelope { type = type, data = data ?? string.Empty }));
            var length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.Length));
            await _sendLock.WaitAsync();
            try
            {
                await _stream.WriteAsync(length, 0, length.Length);
                await _stream.WriteAsync(payload, 0, payload.Length);
                await _stream.FlushAsync();
            }
            catch (Exception error) { _mainThreadActions.Enqueue(() => SetStatus("发送失败：" + error.Message)); }
            finally { _sendLock.Release(); }
        }

        private void DisconnectSocketOnly()
        {
            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }
            try { _listener?.Stop(); } catch { }
            _stream = null; _client = null; _listener = null;
        }

        private bool IsCurrentOperation(int operation)
        {
            return Volatile.Read(ref _connectionOperation) == operation;
        }

        private static void StopListener(TcpListener listener)
        {
            try { listener?.Stop(); } catch { }
        }

        private void SetStatus(string value)
        {
            Status = value;
            StatusChanged?.Invoke(value);
        }

        private static void ParseEndpoint(string address, out string host, out int port)
        {
            address = (address ?? string.Empty).Trim();
            if (address.StartsWith("tcp://", StringComparison.OrdinalIgnoreCase)) address = address.Substring(6);
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("请输入 SakuraFRP 地址");
            var separator = address.LastIndexOf(':');
            if (separator > 0 && int.TryParse(address.Substring(separator + 1), out port))
                host = address.Substring(0, separator).Trim('[', ']');
            else { host = address; port = HostPort; }
        }

        private static string Hash(byte[] data)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", string.Empty);
        }

        [Serializable] private sealed class Envelope { public string type; public string data; }
        [Serializable] private sealed class TextureManifest { public TextureManifestEntry[] entries = Array.Empty<TextureManifestEntry>(); }
        [Serializable] private sealed class TextureManifestEntry { public int shipId; public string hash; public int length; }
        [Serializable] private sealed class TextureChunk { public int shipId; public string hash; public int index; public int total; public string data; }

        private const int TextureChunkSize = 24 * 1024;
        private const int MaxMessageSize = 2 * 1024 * 1024;
        private const int HostPortCheckTimeoutMilliseconds = 15 * 1000;
        private const int HostPortRetryMilliseconds = 250;
        private readonly IDatabase _database;
        private readonly PlayerFleet _playerFleet;
        private readonly ConcurrentQueue<Envelope> _incoming = new();
        private readonly ConcurrentQueue<Action> _mainThreadActions = new();
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private readonly Dictionary<int, TextureManifestEntry> _expectedTextures = new();
        private readonly Dictionary<int, byte[][]> _textureChunks = new();
        private List<IShip> _remoteFleet = new();
        private TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cancel;
        private int _connectionOperation;
        private bool _resourcesPreparedSent;
        private bool _peerResourcesPrepared;
        private bool _localReady;
        private bool _peerReady;
        private bool _manifestReceived;
    }

    [Serializable] public sealed class NetInput
    {
        public int sequence;
        public float throttle;
        public bool hasCourse;
        public float course;
        public string systems;
    }

    [Serializable] public sealed class ShipSnapshot
    {
        public int owner;
        public int slot;
        public float x;
        public float y;
        public float rotation;
        public float armor;
        public float shield;
        public float energy;
        public float throttle;
        public bool hasCourse;
        public float course;
        public string systems;
    }

    [Serializable] public sealed class SnapshotBatch
    {
        public int sequence;
        public ShipSnapshot[] ships;
    }
}
