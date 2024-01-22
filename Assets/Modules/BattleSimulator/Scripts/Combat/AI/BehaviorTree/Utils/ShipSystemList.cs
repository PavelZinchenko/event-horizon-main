using System.Collections.Generic;
using Combat.Component.Systems;

namespace Combat.Ai.BehaviorTree.Utils
{
	public struct ShipSystemList<T> where T : ISystem
	{
		private SystemData _first;
		private List<SystemData> _systems;
		private bool _hasValue;

		public int Count => _hasValue ? (_systems == null ? 1 : _systems.Count + 1) : 0;
		public SystemData this[int index] => index == 0 ? _first : _systems[index - 1];

		public void Add(T value, int id)
		{
			var data = new SystemData(value, id);
			if (!_hasValue)
			{
				_first = data;
				_hasValue = true;
			}
			else
			{
				(_systems ??= new()).Add(data);
			}
		}

		public readonly struct SystemData
		{
			public SystemData(T value, int id) { Value = value; Id = id; }
			public readonly T Value;
			public readonly int Id;
		}
	}
}
