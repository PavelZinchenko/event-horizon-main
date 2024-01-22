using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Combat.Ai.BehaviorTree.Utils
{
	public class EmptyList<T> : IReadOnlyList<T>
	{
		public static readonly IReadOnlyList<T> Instance = new EmptyList<T>();
		public T this[int index] => throw new System.IndexOutOfRangeException();
		public int Count => 0;
		public IEnumerator<T> GetEnumerator() => Enumerable.Empty<T>().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Enumerable.Empty<T>().GetEnumerator();
	}
}
