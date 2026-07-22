namespace EditorDatabase.Model
{
    public class Wrapper<T> where T : class
    {
        public ItemId<T> Item = ItemId<T>.Empty;

        public override string ToString()
        {
            return Item.ToString();
        }
    }

	public class ValueWrapper<T> where T : struct
	{
		public T Value;

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public interface IObjectWrapper
	{
		object CurrentValue { get; }
		object DefaultValue { get; }
		void CreateNew();
		void Clear();
	}

	public class ObjectWrapper<T> : IObjectWrapper where T : class, new()
	{
		public T Value;
		private T _defaultValue;

		public object CurrentValue => Value;
		public object DefaultValue => _defaultValue;
		public void CreateNew() => Value = new();
		public void Clear() => Value = _defaultValue;

		public ObjectWrapper(T defaultValue)
		{
			_defaultValue = defaultValue;
			Value = defaultValue;
		}

		public override string ToString()
		{
			return Value?.ToString();
		}
	}
}
