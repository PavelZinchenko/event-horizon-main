using System.Collections.Generic;
using GameServices.SceneManager;
using GameServices.Player;
using Constructor.Ships;
using Zenject;
using ShipEditor.Context;
using CommonComponents.Utils;
using Constructor.Satellites;
using Constructor;
using GameDatabase.DataModel;
using GameServices.Database;
using GameServices.Research;
using Economy;
using CommonComponents.Signals;

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

			public ShipEditorContext(IShip ship, IInventoryProvider inventory, ITechnologies technologies, Research research)
			{
				Ship = ship;
				Inventory = inventory;
				_research = research;
				_technologies = technologies;
			}

			public IShip Ship { get; }
			public IInventoryProvider Inventory { get; }
            public bool IsShipNameEditable => true;

            public bool CanBeUnlocked(Component component)
            {
				// TODO: 
				if (component.Id.Value == 96) // Xmas bomb
					return true;

				return _technologies.TryGetComponentTechnology(component, out var tech) && _research.IsTechResearched(tech);
			}
		}

		private class InventoryProvider : IInventoryProvider
		{
			private readonly PlayerInventory _playerInventory;
			private readonly PlayerFleet _playerFleet;
			private readonly PlayerResources _playerResources;

			public IReadOnlyGameItemCollection<ComponentInfo> Components => _playerInventory.Components;
			public IReadOnlyGameItemCollection<Satellite> Satellites => _playerInventory.Satellites;
			public IReadOnlyCollection<ISatellite> SatelliteBuilds => System.Array.Empty<ISatellite>();
			public IEnumerable<IShip> Ships => _playerFleet.Ships;

			public InventoryProvider(PlayerInventory playerInventory, PlayerFleet playerFleet, PlayerResources playerResources)
			{
				_playerInventory = playerInventory;
				_playerResources = playerResources;
				_playerFleet = playerFleet;
			}

			public void AddComponent(ComponentInfo component)
			{
				_playerInventory.Components.Add(component);
			}

			public bool TryRemoveComponent(ComponentInfo component)
			{
				if (_playerInventory.Components.GetQuantity(component) == 0) return false;
				_playerInventory.Components.Remove(component);
				return true;
			}

			public void AddSatellite(Satellite satellite)
			{
				_playerInventory.Satellites.Add(satellite);
			}

			public bool TryRemoveSatellite(Satellite satellite)
			{
				if (_playerInventory.Satellites.GetQuantity(satellite) == 0) return false;
				_playerInventory.Satellites.Remove(satellite);
				return true;
			}

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
