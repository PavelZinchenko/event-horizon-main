using System.Collections.Generic;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class RandomSelectorNode : INode
	{
		private readonly float _cooldown;
		private readonly System.Random _random;
		private readonly List<INode> _children = new();

		private INode _selected;
		private float _elapsedTime;

		public RandomSelectorNode(float cooldown)
		{
			_random = new System.Random();
			_cooldown = cooldown;
			_elapsedTime = cooldown;
		}

		public void Add(INode child) => _children.Add(child);

		public NodeState Evaluate(Context context)
		{
			var result = NodeState.Failure;
			_elapsedTime += context.DeltaTime;
			if (_elapsedTime >= _cooldown)
			{
				_selected = null;
				_children.Shuffle(_random);
				_elapsedTime = 0;

				foreach (var child in _children)
				{
					result = child.Evaluate(context);
					if (result == NodeState.Failure) continue;
					_selected = child;
					break;
				}
			}
			else if(_selected != null)
			{
				result = _selected.Evaluate(context);
			}

			return result;
		}
	}
}
