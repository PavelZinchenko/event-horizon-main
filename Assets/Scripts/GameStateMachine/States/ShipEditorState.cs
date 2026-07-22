using System.Collections.Generic;
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
using GameDatabase.Model;
using System.Linq;
using System;
using TextAsset = UnityEngine.TextAsset;
using Debug = UnityEngine.Debug;
using Resources = UnityEngine.Resources;
using JsonUtility = UnityEngine.JsonUtility;

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

            public bool CanBeUnlocked(GameDatabase.DataModel.Component component)
            {
                if (!_technologies.TryGetComponentTechnology(component, out var tech))
                    return false;

                return !tech.RequiredToUnlock || _research.IsTechResearched(tech);
			}
		}

        private class UpgradesProvider : IComponentUpgradesProvider
        {
            public IEnumerable<ComponentUpgradeLevel> GetAllUpgrades() => Enumerable.Empty<ComponentUpgradeLevel>();
            public IComponentUpgrades GetComponentUpgrades(GameDatabase.DataModel.Component component) => null;
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
                _shipPresets = session.ShipPresets.Presets
                    .Select(item => item.ToShipPreset(database))
                    .Where(item => item != null)
                    .ToList();
                EnsureBundledTitanPreset();
            }

            private void EnsureBundledTitanPreset()
            {
                const int titanId = 1145140;
                const string presetName = "$TrisolarisTitan";
                if (_shipPresets.Any(item => item.Ship != null && item.Ship.Id.Value == titanId &&
                                             string.Equals(item.Name, presetName, StringComparison.Ordinal)))
                    return;

                var ship = _database.GetShip(new ItemId<Ship>(titanId));
                if (ship == null || ship == Ship.DefaultValue)
                    return;

                var asset = Resources.Load<TextAsset>("ShipEditor/Presets/TrisolarisTitan");
                if (asset == null)
                {
                    Debug.LogWarning("Bundled Trisolaris Titan preset is missing");
                    return;
                }

                try
                {
                    var data = JsonUtility.FromJson<BundledPresetFile>(asset.text);
                    if (data == null || data.components == null || data.components.Length == 0)
                        return;

                    var preset = new ShipPreset(ship) { Name = presetName };
                    foreach (var item in data.components)
                    {
                        try
                        {
                            var info = ComponentInfo.FromInt64(_database, item.component);
                            if (!info)
                                continue;
                            preset.Components.Add(new IntegratedComponent(info, item.x, item.y,
                                item.barrelId, item.keyBinding, item.behaviour, item.locked));
                        }
                        catch (Exception error)
                        {
                            // A component introduced by a missing optional mod
                            // should not prevent the rest of the Titan preset
                            // from being offered.
                            Debug.LogWarning("Skipping Titan preset component: " + error.Message);
                        }
                    }

                    if (preset.Components.Count == 0)
                        return;

                    _shipPresets.Add(preset);
                    Persist();
                }
                catch (Exception error)
                {
                    Debug.LogWarning("Unable to load bundled Titan preset: " + error.Message);
                }
            }

            [Serializable]
            private sealed class BundledPresetFile
            {
                public BundledPresetComponent[] components;
            }

            [Serializable]
            private sealed class BundledPresetComponent
            {
                public long component;
                public int x;
                public int y;
                public int barrelId;
                public int keyBinding;
                public int behaviour;
                public bool locked;
            }

            public IShipPreset Create(Ship ship)
            {
                var preset = new ShipPreset(ship);
                _shipPresets.Add(preset);
                Persist();
                return preset;
            }

            public void Update(IShipPreset preset)
            {
                // The object is already held by _shipPresets.  Re-serializing
                // here is important because satellite layouts are edited after
                // Create() and Android may leave the editor without disposing
                // this state first.
                if (preset != null && _shipPresets.Contains(preset))
                    Persist();
            }

            public void Delete(IShipPreset preset)
            {
                _shipPresets.Remove(preset);
                Persist();
            }

            public IEnumerable<IShipPreset> GetPresets(Ship ship)
            {
                return _shipPresets.Where(item => item.Ship == ship);
            }

            public void Dispose()
            {
                Persist();
            }

            private void Persist()
            {
                // Presets (including both satellite layouts) used to be copied
                // into session data only when the editor state was disposed.
                // Android can tear down the state without disposing the Zenject
                // container, so save immediately after every create/delete and
                // also after the final edit.
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
