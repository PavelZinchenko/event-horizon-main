using System.Linq;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Domain;
using GameDatabase.Enums;
using Gui.Combat;
using Services.Audio;
using Services.Messenger;
using UnityEngine;
using Zenject;

namespace Combat.Manager
{
    public class CombatTimer : ITickable
    {
        [Inject] private readonly Settings _settings;
        [Inject] private readonly ISoundPlayer _soundPlayer;
        [Inject] private readonly IMusicPlayer _musicPlayer;
        [Inject] private readonly Background.CombatBackground _background;
        [Inject] private readonly TimerPanel _timerPanel;
        [Inject] private readonly CombatManager _combatManager;
        [Inject] private readonly ICombatModel _combatModel;

        [Inject]
        public CombatTimer(IMessenger messenger)
        {
            messenger.AddListener<IShip>(EventType.CombatShipCreated, OnShipCreated);
            messenger.AddListener<IShip>(EventType.CombatShipDestroyed, OnShipDestroyed);
        }

        public int TimeLeft
        {
            get
            {
                return _elapsedTime > _combatModel.Rules.TimeLimit ? 0 : Mathf.CeilToInt(_combatModel.Rules.TimeLimit - _elapsedTime);
            }
        }

        public void ResetTimer()
        {
            _elapsedTime = 0;
        }

        private void OnShipCreated(IShip ship)
        {
            if (ship.Type.Class != UnitClass.Ship)
                return;

            switch (ship.Type.Side)
            {
                case UnitSide.Enemy:
                    _activeEnemyShipCount = _combatModel.EnemyFleet.Ships.Count(item => item.Status == ShipStatus.Active);
                    ResetTimer();
                    break;
                case UnitSide.Player:
                    _activePlayerShip = ship;
                    _activePlayerShipCount = _combatModel.PlayerFleet.Ships.Count(item => item.Status == ShipStatus.Active);
                    break;
            }
        }

        private void OnShipDestroyed(IShip ship)
        {
            if (ship.Type.Class != UnitClass.Ship)
                return;

            switch (ship.Type.Side)
            {
                case UnitSide.Player:
                    if (_combatModel.Rules.TimeOutMode == TimeOutMode.DrainPlayerHp || ship.Type.Side == UnitSide.Enemy)
                        ResetTimer();

                    _activePlayerShipCount = _combatModel.PlayerFleet.Ships.Count(item => item.Status == ShipStatus.Active);
                    break;
                case UnitSide.Enemy:
                    if (_combatModel.Rules.TimeOutMode != TimeOutMode.CallNextEnemyOrDraw || _combatModel.EnemyFleet.IsAnyShipLeft())
                        ResetTimer();

                    _activeEnemyShipCount = _combatModel.EnemyFleet.Ships.Count(item => item.Status == ShipStatus.Active);
                    break;
            }
        }

        public void Tick()
        {
            if (_combatModel == null)
                return;

            _elapsedTime += Time.deltaTime;
            _background.OutOfTimeMode = false;

            if (_activeEnemyShipCount == 0 || _activePlayerShipCount == 0)
            {
                _timerPanel.Enabled = false;
            }
            else if (_combatModel.Rules.TimeLimit <= 0)
            {
                _timerPanel.Enabled = true;
                _timerPanel.Time = 0;
            }
            else
            {
                var timeLeft = Mathf.CeilToInt(_combatModel.Rules.TimeLimit - _elapsedTime);
                _timerPanel.Enabled = true;

                switch (_combatModel.Rules.TimeOutMode)
                {
                    case TimeOutMode.CallNextEnemy:
                        if (_combatManager.CanCallNextEnemy())
                        {
                            if (timeLeft <= 0)
                            {
                                _combatManager.CallNextEnemy();
                                ResetTimer();
                            }
                        }
                        else
                        {
                            timeLeft = 0;
                        }

                        break;
                    case TimeOutMode.CallNextEnemyOrDraw:
                        var hasMoreShips = _combatModel.EnemyFleet.IsAnyShipLeft();
                        if (!hasMoreShips)
                            timeLeft += 2 * _combatModel.Rules.TimeLimit;

                        if (hasMoreShips)
                        {
                            if (timeLeft <= 0 && _combatManager.CanCallNextEnemy())
                            {
                                _combatManager.CallNextEnemy();
                                ResetTimer();
                            }
                        }
                        else if (timeLeft > 0 && timeLeft < 15)
                        {
                            _background.OutOfTimeMode = true;
                        }
                        else if (timeLeft <= 0)
                        {
                            _combatManager.Exit();
                        }

                        break;
                    case TimeOutMode.DrainPlayerHp:
                        if (timeLeft <= 0)
                        {
                            _activePlayerShip.Stats.Armor.Get(-timeLeft * Time.deltaTime * _activePlayerShip.Stats.Armor.MaxValue / 100f);
                            _activePlayerShip.Stats.Shield.Get(-timeLeft * Time.deltaTime * _activePlayerShip.Stats.Shield.MaxValue / 100f);
                            _background.OutOfTimeMode = true;
                        }
                        break;
                }

                _timerPanel.Enabled = true;
                _timerPanel.Time = Mathf.Max(0, timeLeft);
            }

            SetAlarm(_background.OutOfTimeMode);
        }

        private void SetAlarm(bool value)
        {
            if (_isAlarmEnabled != value)
            {
                _isAlarmEnabled = value;
                _musicPlayer.Mute(value);
                if (_isAlarmEnabled)
                    _soundPlayer.Play(_settings.AlarmSound, GetHashCode(), true);
                else
                    _soundPlayer.Stop(GetHashCode());
            }
        }

        private IShip _activePlayerShip;
        private int _activeEnemyShipCount;
        private int _activePlayerShipCount;

        private bool _isAlarmEnabled;
        private float _elapsedTime;
    }
}
