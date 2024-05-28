﻿using System.Collections.Generic;
using GameServices.SceneManager;
using GameServices.Player;
using Constructor.Ships;
using Zenject;
using ShipEditor.Context;
using Constructor.Satellites;
using Constructor;
using GameDatabase.DataModel;
using GameServices.Database;
using GameServices.Research;
using Economy;
using CommonComponents.Signals;
using Session;
using GameDatabase;
using System.Linq;
using System;

namespace GameStateMachine.States
{
    public class ShipEditorState : BaseState
    {
		private Context _context;
		private readonly ShipEditor.CloseEditorSignal _closeEditorSignal;

		public ShipEditorState(
			Context context,
			IStateMachine stateMachine,
			GameStateFactory stateFactory)
            : base(stateMachine, stateFactory)
        {
			_context = context;
			_closeEditorSignal = new();
			_closeEditorSignal.Event += OnExit;
        }

		public override StateType Type => StateType.ShipEditor;

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.ShipEditor; } }

		public override void InstallBindings(DiContainer container)
		{
			if (_context.DatabaseMode)
			{
				container.Bind<IShipEditorContext>().To<DatabaseEditorContext>().AsSingle().WithArguments(_context.Ship);
			}
			else
			{
				container.BindInterfacesTo<InventoryProvider>().AsTransient().WhenInjectedInto<ShipEditorContext>();
                container.Bind<IShipEditorContext>().To<ShipEditorContext>().AsSingle().WithArguments(_context.Ship);
                container.BindInterfacesTo<PresetStorage>().AsSingle();
            }

            container.BindSignal(_closeEditorSignal);
			container.BindTrigger<ShipEditor.CloseEditorSignal.Trigger>();
		}

		private void OnExit()
		{
			LoadState(_context.NextState);
		}

		public struct Context
		{
			public IShip Ship;
			public bool DatabaseMode;
			public IGameState NextState;
		}

		private class ShipEditorContext : IShipEditorContext
		{
			private readonly ITechnologies _technologies;
			private readonly Research _research;

			public ShipEditorContext(IShip ship, IInventoryProvider inventory, IShipPresetStorage presetStorage, ITechnologies technologies, Research research)
			{
				Ship = ship;
				Inventory = inventory;
				_research = research;
				_technologies = technologies;
                ShipPresetStorage = presetStorage;
                UpgradesProvider = new UpgradesProvider();
			}

			public IShip Ship { get; }
			public IInventoryProvider Inventory { get; }
            public IShipDataProvider ShipDataProvider => new EmptyDataProvider();
            public bool IsShipNameEditable => true;
            public IShipPresetStorage ShipPresetStorage { get; }
            public IComponentUpgradesProvider UpgradesProvider { get; }

            public bool CanBeUnlocked(Component component)
            {
				// TODO: 
				if (component.Id.Value == 96) // Xmas bomb
					return true;

				return _technologies.TryGetComponentTechnology(component, out var tech) && _research.IsTechResearched(tech);
			}
		}

        private class UpgradesProvider : IComponentUpgradesProvider
        {
            public IEnumerable<ComponentUpgradeLevel> GetAllUpgrades() => Enumerable.Empty<ComponentUpgradeLevel>();
            public IComponentUpgrades GetComponentUpgrades(Component component) => null;
        }

        private class PresetStorage : IShipPresetStorage, IDisposable
        {
            private readonly List<IShipPreset> _shipPresets;
            private readonly ISessionData _session;
            private readonly IDatabase _database;

            public PresetStorage(ISessionData session, IDatabase database)
            {
                _session = session;
                _database = database;
                _shipPresets = session.ShipPresets.Presets.Select(item => item.ToShipPreset(database)).ToList();
            }

            public IShipPreset Create(Ship ship)
            {
                var preset = new ShipPreset(ship);
                _shipPresets.Add(preset);
                return preset;
            }

            public void Delete(IShipPreset preset)
            {
                _shipPresets.Remove(preset);
            }

            public IEnumerable<IShipPreset> GetPresets(Ship ship)
            {
                return _shipPresets.Where(item => item.Ship == ship);
            }

            public void Dispose()
            {
                _session.ShipPresets.UpdatePresets(_shipPresets);
            }
        }

        private class InventoryProvider : IInventoryProvider
        {
            private readonly PlayerInventory _playerInventory;
            private readonly PlayerFleet _playerFleet;
            private readonly PlayerResources _playerResources;

            public IReadOnlyCollection<ISatellite> SatelliteBuilds => Array.Empty<ISatellite>();
            public IEnumerable<IShip> Ships => _playerFleet.Ships;

            public InventoryProvider(PlayerInventory playerInventory, PlayerFleet playerFleet, PlayerResources playerResources)
            {
                _playerInventory = playerInventory;
                _playerResources = playerResources;
                _playerFleet = playerFleet;
            }

            public IReadOnlyCollection<ComponentInfo> Components => _playerInventory.Components.Keys;
            public int GetQuantity(ComponentInfo component) => _playerInventory.Components.GetQuantity(component);
            public void AddComponent(ComponentInfo component) => _playerInventory.Components.Add(component);
            public bool TryRemoveComponent(ComponentInfo component) => _playerInventory.Components.Remove(component) > 0;

            public IReadOnlyCollection<Satellite> Satellites => _playerInventory.Satellites.Keys;
            public int GetQuantity(Satellite satellite) => _playerInventory.Satellites.GetQuantity(satellite);
            public void AddSatellite(Satellite satellite) => _playerInventory.Satellites.Add(satellite);
            public bool TryRemoveSatellite(Satellite satellite) => _playerInventory.Satellites.Remove(satellite) > 0;

            public Price GetUnlockPrice(ComponentInfo component)
            {
                return component.Price * 2;
            }

            public bool TryPayForUnlock(ComponentInfo component)
            {
                return GetUnlockPrice(component).TryWithdraw(_playerResources);
            }
        }

        public class Factory : PlaceholderFactory<Context, ShipEditorState> { }
    }
}
