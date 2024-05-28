using Combat.Ai.BehaviorTree.Nodes;
using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree
{
	public class ShipBehaviorTree
	{
		private readonly Context _context;
		private readonly INode _rootNode;
		private readonly IShip _ship;
        private bool _lastResult;

		public ShipBehaviorTree(IShip ship, INode rootNode, Context context)
		{
			_ship = ship;
			_rootNode = rootNode;
			_context = context;
		}

		public void Update(float deltaTime, in AiManager.Options options)
		{
			_context.Update(deltaTime, options);

			var result = _rootNode.Evaluate(_context) != NodeState.Failure;
			if (result)
			{
				_context.Controls.Apply(_ship);
			}
			else
			{
				_context.Controls.Reset();
				if (_lastResult)
					_context.Controls.Apply(_ship);
			}

			_lastResult = result;
		}
	}
}
