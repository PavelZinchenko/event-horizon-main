using Combat.Ai.BehaviorTree.Nodes;
using Combat.Component.Ship;
using Combat.Scene;

namespace Combat.Ai.BehaviorTree
{
	public class ShipBehaviorTree
	{
		private readonly Context _context;
		private readonly INode _rootNode;
		private readonly IShip _ship;
		private bool _lastResult;

		public ShipBehaviorTree(IShip ship, IScene scene, INode rootNode)
		{
			_ship = ship;
			_rootNode = rootNode;
			_context = new Context(ship, scene);
		}

		public void Update(float deltaTime)
		{
			_context.Update(deltaTime);
			var result = _rootNode.Evaluate(_context) == NodeState.Success;
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
