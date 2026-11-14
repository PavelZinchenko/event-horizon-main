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
                Open(() => RelayTransport.ListenDirect(port));
        }

        public void JoinGameDirect()
        {
            if (CanStart() && TryGetPort(out var port))
                Open(() => RelayTransport.ConnectDirect(_address.Trim(), port));
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

            if (_handshake?.State == HandshakeState.Rejected)
                Stop(RejectionKey(_handshake.Rejection));
            else if (_handshake?.State == HandshakeState.Ready)
                StartDuel();
            else if (_transport.State == LinkState.Closed)
                Stop("$DuelConnectionClosed");
            else
                ShowProgress();
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

        private void Open(Func<RelayTransport> create)
        {
            try
            {
                _transport = create();
                SetStatus(null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                SetStatus("$DuelConnectionError");
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
            SetStatus("$DuelConnectionError");
        }

        private MatchHandshake CreateHandshake()
        {
            try
            {
                var mapper = new CoreShipMapper(_database, _resourceLocator);
                _duelSetup = new CoreDuelSetup(_database, mapper);
                var fleet = FleetWebSerializer.SerializeFleetData(_playerFleet.ActiveShipGroup.Ships);
                var identity = new MatchIdentity(Application.version, _database.ContentHash, fleet);
                if (!_transport.IsHost) return MatchHandshake.Guest(_transport, identity, _duelSetup.Build);
                var random = new System.Random();
                return MatchHandshake.Host(_transport, identity, _duelSetup.Build,
                    new CombatSimulation.Core.BattleId((ulong)random.Next(1, int.MaxValue)), (ulong)random.Next());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Stop("$DuelConnectionError");
                return null;
            }
        }

        // From here the duel owns the transport: the session thread starts at once and the combat state disposes it.
        private void StartDuel()
        {
            CoreDuelBattle duel;
            try
            {
                duel = new CoreDuelBattle(_handshake, _duelSetup, _transport, Now);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Stop("$DuelConnectionError");
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

        private void SetStatus(string key)
        {
            if (key == _statusKey) return;
            _statusKey = key;
            _statusText.text = key == null ? string.Empty : _localization.GetString(key);
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
                default:
                    return "$DuelRejectedSetup";
            }
        }

        private static string ResultKey(OnlineDuelOutcome outcome)
        {
            switch (outcome)
            {
                case OnlineDuelOutcome.Victory: return "$DuelVictory";
                case OnlineDuelOutcome.Defeat: return "$DuelDefeat";
                case OnlineDuelOutcome.Draw: return "$DuelDraw";
                default: return "$DuelNoResult";
            }
        }

        private string _joinCode;
        private string _address;
        private string _port;
        private string _relayCode;
        private string _statusKey = string.Empty;
        private CancellationTokenSource _pending;
        private RelayTransport _transport;
        private MatchHandshake _handshake;
        private CoreDuelSetup _duelSetup;
    }
}
