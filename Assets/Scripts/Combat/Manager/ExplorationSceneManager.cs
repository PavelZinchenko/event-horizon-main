﻿using System;
using System.Collections.Generic;
using System.Linq;
using Combat.Collision;
using Combat.Domain;
using Combat.Component.Triggers;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Factory;
using Combat.Scene;
using Combat.Unit;
using Combat.Unit.Object;
using Economy.Products;
using Game.Exploration;
using GameStateMachine.States;
using Services.Messenger;
using GameDatabase;
using GameServices.Player;
using Services.Gui;
using Services.Resources;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;
using IShip = Combat.Component.Ship.IShip;

namespace Combat.Manager
{
    public class ExplorationSceneManager : ITickable, IInitializable, IDisposable
    {
        [Inject]
        private ExplorationSceneManager(
            IMessenger messenger)
        {
            _messenger = messenger;

            _messenger.AddListener<IShip>(EventType.CombatShipCreated, OnShipCreated);
            _messenger.AddListener<IShip>(EventType.CombatShipDestroyed, OnShipDestroyed);
            _messenger.AddListener(EventType.Surrender, Exit);
            _messenger.AddListener(EventType.KillAllEnemies, KillThemAll);
            _messenger.AddListener<int>(EventType.PlayerShipDocked, OnPlayerDocked);
            _messenger.AddListener<int>(EventType.PlayerShipUndocked, OnPlayerUndocked);
            _messenger.AddListener<int>(EventType.ObjectiveDestroyed, OnObjectiveDestroyed);
        }

        [Inject] private readonly IScene _scene;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly Gui.Combat.ShipControlsPanel _shipControlsPanel;
        [Inject] private readonly ShipFactory _shipFactory;
        [Inject] private readonly ControllerFactory _controllerFactory;
        [Inject] private readonly SpaceObjectFactory _spaceObjectFactory;
        [Inject] private readonly EffectFactory _effectFactory;
        [Inject] private readonly IGuiManager _guiManager;
        [Inject] private readonly PlayerFleet _playerFleet;
        [Inject] private readonly PlayerSkills _playerSkills;
        [Inject] private readonly ExplorationData _exploration;
        [Inject] private readonly ExitSignal.Trigger _exitTrigger;

        [Inject] private readonly Gui.Combat.ShipStatsPanel _playerStatsPanel;
        [Inject] private readonly Gui.Combat.ShipStatsPanel _enemyStatsPanel;
        [Inject] private readonly Gui.Combat.RadarPanel _radarPanel;

        private void OnShipCreated(IShip ship)
        {
            if (ship.Type.Class != UnitClass.Ship)
                return;

            switch (ship.Type.Side)
            {
                case UnitSide.Player:
                    _shipControlsPanel.Load(ship);
                    break;
                case UnitSide.Enemy:
                    _radarPanel.Add(ship);
                    break;
            }
        }

        private void OnShipDestroyed(IShip ship)
        {
            if (ship.Type.Class != UnitClass.Ship)
                return;

            if (ship.Type.Side == UnitSide.Player)
                DelayedExit();
        }

        private void OnPlayerDocked(int stationId)
        {
            Debug.Log("OnPlayerDocked - " + stationId);

            if (!_objectives.ContainsKey(stationId)) return;
            _guiManager.OpenWindow(Gui.Exploration.WindowNames.ScanningPanel, code => { if (code == WindowExitCode.Ok) OnScanCompleted(stationId); });
        }

        private void OnObjectiveDestroyed(int id)
        {
            Debug.Log("OnObjectiveDestroyed - " + id);
            OnScanCompleted(id);
        }

        private void OnPlayerUndocked(int stationId)
        {
            Debug.Log("OnPlayerUndocked - " + stationId);
        }

        private void OnScanCompleted(int stationId)
        {
            Debug.Log("OnScanCompleted- " + stationId);

            if (!_objectives.TryGetValue(stationId, out var unit))
                return;

            _objectives.Remove(stationId);
            _radarPanel.RemoveBeacon(unit);
            _exploration.Complete(stationId);

            var objective = _exploration.Objectives[stationId];
            var loot = _exploration.GetLoot(objective).ToList();
            loot.Consume();
            _guiManager.OpenWindow(Gui.Exploration.WindowNames.LootPanel, new WindowArgs(loot));

            if (_objectives.Count == 0)
                DelayedExit();
        }

        public void Tick()
        {
            _playerStatsPanel.Open(_scene.PlayerShip);

            if (_scene.EnemyShip.IsActive())
                _enemyStatsPanel.Open(_scene.EnemyShip);
            else
                _enemyStatsPanel.Close();
        }

        private void DelayedExit()
        {
            Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(_ => Exit());
        }

        private void Exit()
        {
			_scene.Clear();
			_exitTrigger.Fire();
        }

        private void KillThemAll()
        {
            foreach (var ship in _scene.Ships.Items)
                if (ship.Type.Side.IsEnemy(UnitSide.Player))
                    ship.Affect(new Impact { Effects = CollisionEffect.Destroy }, null);
        }

        public void Initialize()
        {
            CreatePlayerShip();
            _radarPanel.Initialize(_scene.Units.Items);
            BuildLevel();
        }

        public void Dispose()
        {
        }

        private void CreatePlayerShip()
        {
            var spec = _playerFleet.ExplorationShip.CreateBuilder().ApplyPlayerSkills(_playerSkills).Build(_database.ShipSettings);
            var ship = _shipFactory.CreatePlayerShip(spec, Vector2.zero, new System.Random().Next(360));

            _shipControlsPanel.Load(ship);
        }

        private void BuildLevel()
        {
            var random = new System.Random(_exploration.Seed);
            CreateObjectives(_exploration.Seed + 1);
            CreateEnemies(_exploration.Seed + 2);
            CreateEnvironment(_exploration.Seed + 3);
        }

        private void CreateEnemies(int seed)
        {
            var random = new System.Random(seed);
            foreach (var enemy in _exploration.EnemyShips)
            {
                var position = Vector2.zero;
                while (position.SqrDistance(Vector2.zero) < _enemyActivationDistance * _enemyActivationDistance)
                    position = new Vector2(random.NextFloatSigned() * 0.5f * _scene.Settings.AreaWidth, random.NextFloatSigned() * 0.5f * _scene.Settings.AreaHeight);

                var spawner = new EnemySpawner(position, _enemyActivationDistance, _enemyDeactivationDistance, _scene, enemy, _shipFactory, _effectFactory, _spaceObjectFactory);
                _scene.AddUnit(spawner);
                _sceneObjects.Add(spawner);
            }
        }

        private void CreateObjectives(int seed)
        {
            const int mapSize = 10;
            var objectives = _exploration.Objectives;
            Assert.IsTrue(objectives.Count < mapSize * mapSize);

            var random = new System.Random(seed);
            var positions = EnumerableExtension.RandomUniqueNumbers(1, mapSize * mapSize, objectives.Count, random).ToArray();
            for (var i = 0; i < positions.Length; ++i)
            {
                var objective = objectives[i];
                if (_exploration.IsCompleted(objective)) continue;

                var y = ((positions[i] / mapSize) / (float)mapSize) * _scene.Settings.AreaWidth + 0.25f * random.NextFloatSigned() * _scene.Settings.AreaWidth / mapSize;
                var x = ((positions[i] % mapSize) / (float)mapSize) * _scene.Settings.AreaHeight + 0.25f * random.NextFloatSigned() * _scene.Settings.AreaHeight / mapSize;
                var position = new Vector2(x, y);
                var rotation = random.Next(360);
                var defaultColor = Color.Lerp(_exploration.PlanetColor, Color.white, 0.5f);

                IUnit unit;
                switch (objective.Type)
                {
                    case ObjectiveType.Container:
                        {
                            var size = 4 + random.NextFloat()*3;
                            var box = _exploration.HasSolidGround 
                                ? _spaceObjectFactory.CreatePlanetaryContainer(position, rotation, size, defaultColor) 
                                : _spaceObjectFactory.CreatePlanetaryFloatingContainer(position, size, defaultColor);

                            box.AddTrigger(new PlayerDockingAction(_messenger, i));
                            unit = box;
                        }
                        break;
                    case ObjectiveType.Meteorite:
                        {
                            var size = 5 + random.NextFloat() * 5;
                            var asteroid = _exploration.HasSolidGround
                                ? _spaceObjectFactory.CreatePlanetaryRock(position, size, _exploration.PlanetColor)
                                : _spaceObjectFactory.CreatePlanetaryFloatingRock(position, size, _exploration.PlanetColor);
                            asteroid.AddTrigger(new PlayerDockingAction(_messenger, i));
                            unit = asteroid;
                        }
                        break;
                    case ObjectiveType.ShipWreck:
                        {
                            var ship = _exploration.GetShipWreck(objective.Seed);
                            var sprite = _resourceLocator.GetSprite(ship.ModelImage);
                            var size = 5 + ship.ModelScale;
                            var angularVelocity = _exploration.HasSolidGround ? 0 : random.NextFloatSigned()*0.1f;
                            var wreck = _spaceObjectFactory.CreatePlanetaryShipWreck(sprite, position, rotation, angularVelocity, size, defaultColor);
                            wreck.AddTrigger(new PlayerDockingAction(_messenger, i));
                            unit = wreck;
                        }
                        break;
                    case ObjectiveType.Minerals:
                        {
                            var size = 5 + random.NextFloat()*10;
                            var item = _exploration.HasSolidGround
                                ? _spaceObjectFactory.CreatePlanetaryVolcano(position, rotation, size, Color.Lerp(_exploration.PlanetColor, Color.black, 0.1f))
                                : _spaceObjectFactory.CreatePlanetaryVortex(position, size, _exploration.GetGasCloudColor(i));

                            item.AddTrigger(new PlayerDockingAction(_messenger, i));
                            unit = item;
                        }
                        break;
                    case ObjectiveType.MineralsRare:
                        {
                            var size = 5 + random.NextFloat() * 10;
                            var item = _exploration.HasSolidGround
                                ? _spaceObjectFactory.CreatePlanetaryVolcano2(position, rotation, size, Color.Lerp(_exploration.PlanetColor, Color.black, 0.1f))
                                : _spaceObjectFactory.CreatePlanetaryVortex(position, size, _exploration.GetGasCloudColor(i));

                            item.AddTrigger(new PlayerDockingAction(_messenger, i));
                            unit = item;
                        }
                        break;
                    case ObjectiveType.Outpost:
                        {
                            var builder = _exploration.GetOutpost(objective.Seed);
                            var action = new UnitDestroyedAction(_messenger, i);
                            unit = new EnemySpawner(position, _enemyActivationDistance, _enemyDeactivationDistance, _scene, builder, _shipFactory, _effectFactory, _spaceObjectFactory, action);
                            _scene.AddUnit(unit);

                            var turretCount = 1 + random.Next(4);
                            for (var j = 0; j < turretCount; ++j)
                            {
                                var turretPosition = position + RotationHelpers.Direction(rotation + j*360/turretCount) * 10;
                                var turretBuilder = _exploration.GetTurret(objective.Seed + j);
                                var turret = new EnemySpawner(turretPosition, _enemyActivationDistance, _enemyDeactivationDistance, _scene, turretBuilder, _shipFactory, _effectFactory, _spaceObjectFactory);
                                _scene.AddUnit(turret);
                            }
                        }
                        break;
                    case ObjectiveType.Hive:
                        {
                            var builder = _exploration.GetHive(objective.Seed);
                            var action = new UnitDestroyedAction(_messenger, i);
                            unit = new EnemySpawner(position, _enemyActivationDistance, _enemyDeactivationDistance, _scene, builder, _shipFactory, _effectFactory, _spaceObjectFactory, action);
                            _scene.AddUnit(unit);

                            var guardianCount = 1 + random.Next(4);
                            for (var j = 0; j < guardianCount; ++j)
                            {
                                var turretPosition = position + RotationHelpers.Direction(rotation + j * 360 / guardianCount) * 10;
                                var turretBuilder = _exploration.GetHiveGuardian(random);
                                var guardian = new EnemySpawner(turretPosition, _enemyActivationDistance, _enemyDeactivationDistance, _scene, turretBuilder, _shipFactory, _effectFactory, _spaceObjectFactory);
                                guardian.RecoveryTıme = 30;
                                guardian.Parent = unit;
                                _scene.AddUnit(guardian);
                            }
                        }
                        break;
                    default:
                        throw new ArgumentException();
                }

                _radarPanel.AddBeacon(unit);
                _sceneObjects.Add(unit);
                _objectives.Add(i, unit);
            }
        }

        private void CreateEnvironment(int seed)
        {
            var random = new System.Random(seed);

            for (var i = 0; i < 100; ++i)
            {
                var x = random.NextFloat() * _scene.Settings.AreaWidth;
                var y = random.NextFloat() * _scene.Settings.AreaHeight;
                var position = new Vector2(x, y);
                var size = 5 + random.Next(10);
                var rotation = random.Next(360);
                var color = Color.Lerp(_exploration.PlanetColor, Color.black, 0.1f);

                var type = _exploration.GetEnvironmentObject(random);
                switch (type)
                {
                    case ExplorationData.EnvironmentObjectType.SmallCrater:
                        _spaceObjectFactory.CreatePlanetaryCrater(position, rotation, size, color); 
                        break;
                    case ExplorationData.EnvironmentObjectType.BigCrater:
                        _spaceObjectFactory.CreatePlanetaryCrater2(position, rotation, size, color); 
                        break;
                    case ExplorationData.EnvironmentObjectType.GasCloud:
                        _spaceObjectFactory.CreatePlanetaryGasCloud(position, size * 2, random.NextFloatSigned() * 0.1f, size * 1.5f, 
							_exploration.GetGasCloudColor(i), _database.ExplorationSettings.GasCloudDPS(_exploration.Level));
                        break;
                }
            }
        }

        private readonly IMessenger _messenger;
        private readonly Dictionary<int, IUnit> _objectives = new Dictionary<int, IUnit>();
        private readonly List<IUnit> _sceneObjects = new List<IUnit>();

        private const float _enemyActivationDistance = 75f;
        private const float _enemyDeactivationDistance = 100f;
    }
}
