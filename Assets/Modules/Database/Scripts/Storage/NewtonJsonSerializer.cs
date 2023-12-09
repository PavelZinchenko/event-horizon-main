using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GameDatabase.Storage
{
    public class NewtonJsonSerializer : IJsonSerializer
    {
		private readonly JsonSerializerSettings _settings;
		
		public NewtonJsonSerializer()
        {
            _settings = new JsonSerializerSettings
            {
				ContractResolver = new ContractResolver(),
				Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            };
        }

        public T FromJson<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public string ToJson<T>(T item)
        {
            return JsonConvert.SerializeObject(item, _settings);
        }

		private class ContractResolver : DefaultContractResolver
		{
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				if (member.MemberType == MemberTypes.Property) return null;

				return base.CreateProperty(member, memberSerialization);
			}
		}
    }
}
