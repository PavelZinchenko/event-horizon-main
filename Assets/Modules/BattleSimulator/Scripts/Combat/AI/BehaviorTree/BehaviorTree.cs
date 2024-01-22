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

		public ShipBehaviorTree(IShip ship, IScene scene, INode rootNode)
		{
			_ship = ship;
			_rootNode = rootNode;
			_context = new Context(ship, scene);
		}

		public void Update(float deltaTime)
		{
			_context.Update(deltaTime);
			if (_rootNode.Evaluate(_context) == NodeState.Failure)
				_context.Controls.Reset();
			else
				_context.Controls.Apply(_ship);
		}
	}
}
