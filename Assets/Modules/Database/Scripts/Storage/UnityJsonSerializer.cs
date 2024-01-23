using UnityEngine;

namespace GameDatabase.Storage
{
    public class UnityJsonSerializer : IJsonSerializer
    {
		private NewtonJsonSerializer _newtonJsonSerializer;

        public T FromJson<T>(string data)
        {
			// Temporary workaround: Unity JsonUtility seems to have troubles deserializing recursive classes, using NewtonJson instead
			if (typeof(T) == typeof(Serializable.BehaviorTreeNodeSerializable))
				return (_newtonJsonSerializer ??= new()).FromJson<T>(data);

            return JsonUtility.FromJson<T>(data);
        }

        public string ToJson<T>(T item)
        {
            return JsonUtility.ToJson(item, true).Replace("    ", "  ");
        }
    }
}
