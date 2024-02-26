using Combat.Component.Ship;
using GameDatabase.DataModel;
using Combat.Ai.BehaviorTree;
using Combat.Unit;

namespace Combat.Ai
{
	public class BehaviorTreeController : IController
	{
		private readonly ShipBehaviorTree _behaviorTree;
		private readonly IShip _ship;

		public BehaviorTreeController(ShipBehaviorTree behaviorTree, IShip ship)
		{
			_behaviorTree = behaviorTree;
			_ship = ship;
		}

		public ControllerStatus Status => _ship.IsActive() ? ControllerStatus.Active : ControllerStatus.Dead;

        public void Update(float deltaTime)
        {
            _behaviorTree.Update(deltaTime);
            _ship.Controls.DataChanged = false;
        }

        public class Factory : IControllerFactory
		{
			private readonly AiSettings _settings;
			private readonly BehaviorTreeBuilder _builder;
			private readonly BehaviorTreeModel _behaviorTree;

			public Factory(BehaviorTreeModel behaviorTree, AiSettings settings, BehaviorTreeBuilder builder)
			{
				_builder = builder;
				_settings = settings;
				_behaviorTree = behaviorTree;
			}

			public IController Create(IShip ship)
			{
				return new BehaviorTreeController(_builder.Build(ship, _behaviorTree, _settings), ship);
			}
		}
	}
}
