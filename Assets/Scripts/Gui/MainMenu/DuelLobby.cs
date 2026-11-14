using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Combat.Integration.Core;
using CombatSimulation.Runtime;
using GameServices.Player;
using GameDatabase;
using GameServices.Multiplayer;
using GameStateMachine.States;
using Gui.Windows;
using OnlineDuel;
using Services.Localization;
using Services.Resources;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Gui.MainMenu
{
    // The online duel lobby (phase 7, step 7): host or join over Relay or a direct link, then the handshake with the
    // active ship group (Q3). At handshake Ready it hands the duel to the main menu state and closes; the main menu
    // loads the duel combat state when it resumes. After the duel the lobby reopens with the result in the status line.
    public class DuelLobby : MonoBehaviour
    {
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly PlayerFleet _playerFleet;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly OnlineDuelResult _lastResult;
        [Inject] private readonly StartOnlineDuelSignal.Trigger _startDuelTrigger;

        [SerializeField] private GameObject _relayPanel;
        [SerializeField] private GameObject _directPanel;
        [SerializeField] private GameObject _waitingPanel;
        [SerializeField] private Text _statusText;
        [SerializeField] private GameObject _joinCodePanel;
        [SerializeField] private Text _joinCodeText;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _hostDirectButton;
        [SerializeField] private Button _joinDirectButton;
        [SerializeField] private Button _copyCodeButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private InputField _joinCodeInput;
        [SerializeField] private InputField _addressInput;
        [SerializeField] private InputField _portInput;
        [SerializeField] private Toggle _relayModeToggle;
        [SerializeField] private Toggle _directModeToggle;

        private const string DefaultAddress = "127.0.0.1";
        private const string DefaultPort = "7777";
        // How long a rejecting player keeps the link open so that the peer gets the Reject and shows its reason.
        private const double RejectDeliverySeconds = 2;

        public void CreateGameRelay()
        {
            if (CanStart())
                HostRelay();
        }

        public void JoinGameRelay()
        {
            if (CanStart())
                JoinRelay();
        }

        public void CreateGameDirect()
        {
            if (CanStart() && TryGetPort(out var port))
                Open(() => RelayTransport.ListenDirect(port), "$DuelCannotListen");
        }

        public void JoinGameDirect()
        {
            if (CanStart() && TryGetPort(out var port))
                Open(() => RelayTransport.ConnectDirect(_address.Trim(), port), "$DuelCannotConnect");
        }

        public void Cancel()
        {
            Close();
            UpdateControls();
        }

        public void CopyCode()
        {
            if (!string.IsNullOrEmpty(_relayCode))
                GUIUtility.systemCopyBuffer = _relayCode;
        }

        public void OnJoinCodeChanged(string value) => _joinCode = value;
        public void OnAddressChanged(string value) => _address = value;
        public void OnPortChanged(string value) => _port = value;

        public void OnDirectModeSelected()
        {
            _relayPanel.SetActive(false);
            _directPanel.SetActive(true);
        }

        public void OnRelayModeSelected()
        {
            _relayPanel.SetActive(true);
            _directPanel.SetActive(false);
        }

        // The window is closing: leave the link unless the duel already owns it.
        public void OnClose() => Close();

        private void Start()
        {
            if (string.IsNullOrEmpty(_addressInput.text)) _addressInput.text = DefaultAddress;
            if (string.IsNullOrEmpty(_portInput.text)) _portInput.text = DefaultPort;
            _address = _addressInput.text;
            _port = _portInput.text;
            _joinCode = _joinCodeInput.text;

            var result = _lastResult.Take();
            if (result != OnlineDuelOutcome.None)
                SetStatus(ResultKey(result));
            else if (!_playerFleet.ActiveShipGroup.Ships.Any())
                SetStatus("$DuelNoShips");
            else
                SetStatus(null);

            UpdateControls();
        }

        private void Update()
        {
            if (_transport == null) return;

            _transport.Update();
            if (_handshake == null && _transport.State == LinkState.Connected)
            {
                _handshake = CreateHandshake();
                if (_handshake == null) return;
            }
            _handshake?.Update(Now());
            _transport.Flush();

            if (_rejectedAt.HasValue)
            {
                // Our Reject is on its way: stop once the peer has it, the peer left, or the wait is over.
                if (_handshake.PendingReliablePackets == 0 || _transport.State == LinkState.Closed ||
                    Now() - _rejectedAt.Value > RejectDeliverySeconds)
                    Stop(_statusKey);
            }
            else if (_handshake?.State == HandshakeState.Rejected)
                OnRejected();
            else if (_handshake?.State == HandshakeState.Ready)
                StartDuel();
            else if (_transport.State == LinkState.Closed)
            {
                Debug.LogWarning($"DuelLobby: link closed ({_transport.CloseReason}, timed out: {_transport.TimedOut}, " +
                    $"allocation invalid: {_transport.AllocationInvalid}, handshake: {_handshake?.State})");
                Stop(ClosedKey(_transport));
            }
            else
                ShowProgress();
        }

        private void OnRejected()
        {
            LogRejection(_handshake);
            var key = RejectionKey(_handshake.Rejection);
            // A Reject from the peer, or a silent peer: nothing to deliver.
            if (_handshake.RejectedByPeer || _handshake.Rejection == HandshakeRejection.Timeout)
            {
                Stop(key);
                return;
            }
            SetStatus(key);
            _rejectedAt = Now();
        }

        private void OnDestroy() => Close();

        private static double Now() => Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;

        private bool IsWaiting => _pending != null || _transport != null;

        private bool CanStart()
        {
            if (IsWaiting) return false;
            if (!_playerFleet.ActiveShipGroup.Ships.Any())
            {
                SetStatus("$DuelNoShips");
                return false;
            }
            return true;
        }

        private bool TryGetPort(out ushort port)
        {
            if (ushort.TryParse(_port?.Trim(), out port) && port != 0 && !string.IsNullOrWhiteSpace(_address))
                return true;
            SetStatus("$DuelInvalidAddress");
            return false;
        }

        private void Open(Func<RelayTransport> create, string errorKey)
        {
            try
            {
                _transport = create();
                SetStatus(null);
            }
            catch (ArgumentException e)
            {
                Debug.LogException(e);
                SetStatus("$DuelInvalidAddress");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                SetStatus(errorKey);
            }
            UpdateControls();
        }

        private async void HostRelay()
        {
            var pending = Begin();
            try
            {
                var (transport, joinCode) = await DuelRelay.HostAsync(pending.Token);
                if (End(pending, transport)) _relayCode = joinCode;
            }
            catch (Exception e)
            {
                Fail(pending, e);
            }
            // The scene may have unloaded while the request ran.
            if (this != null) UpdateControls();
        }

        private async void JoinRelay()
        {
            var pending = Begin();
            try
            {
                End(pending, await DuelRelay.JoinAsync(_joinCode ?? string.Empty, pending.Token));
            }
            catch (Exception e)
            {
                Fail(pending, e);
            }
            // The scene may have unloaded while the request ran.
            if (this != null) UpdateControls();
        }

        private CancellationTokenSource Begin()
        {
            _pending = new CancellationTokenSource();
            SetStatus("$DuelRelayConnecting");
            UpdateControls();
            return _pending;
        }

        // False when Cancel or the window closed while the request ran; the transport is then disposed.
        private bool End(CancellationTokenSource pending, RelayTransport transport)
        {
            if (pending.IsCancellationRequested || pending != _pending)
            {
                transport.Dispose();
                return false;
            }
            _pending = null;
            _transport = transport;
            return true;
        }

        private void Fail(CancellationTokenSource pending, Exception e)
        {
            if (pending.IsCancellationRequested || pending != _pending) return;
            _pending = null;
            Debug.LogException(e);
            if (e is DuelServiceException service)
                SetStatus(ServiceErrorKey(service.Error), service.Error == DuelServiceError.Unknown ? service.Code : 0);
            else
                SetStatus("$DuelConnectionError");
        }

        private MatchHandshake CreateHandshake()
        {
            try
            {
                var mapper = new CoreShipMapper(_database, _resourceLocator);
                _duelSetup = new CoreDuelSetup(_database, mapper);
                var fleet = FleetWebSerializer.SerializeFleetData(_playerFleet.ActiveShipGroup.Ships);
                var identity = new MatchIdentity(Application.version, _database.ContentHash, fleet, CoreSourceHash.Value);
                if (!_transport.IsHost) return MatchHandshake.Guest(_transport, identity, _duelSetup.Build);
                var random = new System.Random();
                return MatchHandshake.Host(_transport, identity, _duelSetup.Build,
                    new CombatSimulation.Core.BattleId((ulong)random.Next(1, int.MaxValue)), (ulong)random.Next());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Stop("$DuelPrepareError");
                return null;
            }
        }

        // From here the duel owns the transport: the session thread starts at once and the combat state disposes it.
        private void StartDuel()
        {
            // A warning only: Mono (editor, macOS) and IL2CPP round some derived values differently (MatchHandshake).
            if (_handshake.SetupHashDiffers)
                Debug.LogWarning($"DuelLobby: setup hash differs (local {_handshake.SetupHashValue:X16}, " +
                    $"remote {_handshake.RemoteSetupHash:X16}); the duel continues.");

            CoreDuelBattle duel;
            try
            {
                duel = new CoreDuelBattle(_handshake, _duelSetup, _transport, Now);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Stop("$DuelStartError");
                return;
            }

            _transport = null;
            _handshake = null;
            _relayCode = null;
            _startDuelTrigger.Fire(duel);
            GetComponent<AnimatedWindow>().Close();
        }

        private void ShowProgress()
        {
            if (_handshake != null)
                SetStatus("$DuelPreparing");
            else if (_transport.IsHost)
                SetStatus("$DuelWaitingForPlayer");
            else
                SetStatus("$DuelConnecting");
        }

        private void Stop(string statusKey)
        {
            Close();
            SetStatus(statusKey);
            UpdateControls();
        }

        private void Close()
        {
            _pending?.Cancel();
            _pending = null;
            _relayCode = null;
            _rejectedAt = null;
            _handshake = null;
            _duelSetup = null;
            _transport?.Dispose();
            _transport = null;
        }

        private void UpdateControls()
        {
            var waiting = IsWaiting;
            _waitingPanel.SetActive(waiting);
            _relayModeToggle.interactable = !waiting;
            _directModeToggle.interactable = !waiting;
            _relayPanel.SetActive(!waiting && _relayModeToggle.isOn);
            _directPanel.SetActive(!waiting && _directModeToggle.isOn);
            _hostButton.interactable = !waiting;
            _joinButton.interactable = !waiting;
            _hostDirectButton.interactable = !waiting;
            _joinDirectButton.interactable = !waiting;
            _joinCodeInput.interactable = !waiting;
            _addressInput.interactable = !waiting;
            _portInput.interactable = !waiting;
            _cancelButton.interactable = waiting;

            var hasCode = !string.IsNullOrEmpty(_relayCode);
            _joinCodePanel.SetActive(hasCode);
            _joinCodeText.text = hasCode ? _relayCode : string.Empty;
            _copyCodeButton.gameObject.SetActive(hasCode);
        }

        // code: an error code for the player to report, shown after the text; 0 shows none.
        private void SetStatus(string key, int code = 0)
        {
            if (key == _statusKey && code == _statusCode) return;
            _statusKey = key;
            _statusCode = code;
            _statusText.text = key == null ? string.Empty :
                code == 0 ? _localization.GetString(key) : $"{_localization.GetString(key)} ({code})";
        }

        private static void LogRejection(MatchHandshake handshake)
        {
            Debug.LogWarning($"DuelLobby: handshake rejected ({handshake.Rejection}, " +
                $"{(handshake.RejectedByPeer ? "by the peer" : "here")}, {(handshake.IsHost ? "host" : "guest")})");
            if (handshake.SetupError != null) Debug.LogException(handshake.SetupError);
        }

        private static string RejectionKey(HandshakeRejection rejection)
        {
            switch (rejection)
            {
                case HandshakeRejection.Protocol:
                case HandshakeRejection.GameVersion:
                    return "$DuelRejectedVersion";
                case HandshakeRejection.Database:
                    return "$DuelRejectedDatabase";
                case HandshakeRejection.Source:
                    return "$DuelRejectedBuild";
                case HandshakeRejection.Timeout:
                    return "$DuelNoAnswer";
                case HandshakeRejection.InvalidMessage:
                    return "$DuelRejectedMessage";
                default:
                    return "$DuelRejectedSetup";
            }
        }

        private static string ClosedKey(RelayTransport transport)
        {
            if (transport.AllocationInvalid) return "$DuelRelayLost";
            if (transport.TimedOut) return "$DuelNoAnswer";
            if (transport.CloseReason == Unity.Networking.Transport.Error.DisconnectReason.ClosedByRemote) return "$DuelPeerLeft";
            return "$DuelConnectionClosed";
        }

        private static string ServiceErrorKey(DuelServiceError error)
        {
            switch (error)
            {
                case DuelServiceError.InvalidCode: return "$DuelInvalidCode";
                case DuelServiceError.CodeNotFound: return "$DuelCodeNotFound";
                case DuelServiceError.NoInternet: return "$DuelNoInternet";
                case DuelServiceError.ServiceUnavailable: return "$DuelServiceUnavailable";
                case DuelServiceError.TooManyRequests: return "$DuelTooManyRequests";
                case DuelServiceError.SignInFailed: return "$DuelSignInFailed";
                default: return "$DuelConnectionError";
            }
        }

        private static string ResultKey(OnlineDuelOutcome outcome)
        {
            switch (outcome)
            {
                case OnlineDuelOutcome.Victory: return "$DuelVictory";
                case OnlineDuelOutcome.Defeat: return "$DuelDefeat";
                case OnlineDuelOutcome.Draw: return "$DuelDraw";
                case OnlineDuelOutcome.Disconnected: return "$DuelDisconnected";
                default: return "$DuelNoResult";
            }
        }

        private string _joinCode;
        private string _address;
        private string _port;
        private string _relayCode;
        private string _statusKey = string.Empty;
        private int _statusCode;
        // Set while our Reject is on its way to the peer.
        private double? _rejectedAt;
        private CancellationTokenSource _pending;
        private RelayTransport _transport;
        private MatchHandshake _handshake;
        private CoreDuelSetup _duelSetup;
    }
}
