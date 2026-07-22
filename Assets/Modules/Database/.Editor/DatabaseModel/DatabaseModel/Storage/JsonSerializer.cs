using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EditorDatabase.Storage
{
    public class JsonSerializer : IJsonSerializer
    {
        public JsonSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                ContractResolver = new BaseFirstContractResolver(),
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
            return JsonConvert.SerializeObject(item, _settings).Replace("\r\n", "\n");
        }

        private readonly JsonSerializerSettings _settings;

        private class BaseFirstContractResolver : DefaultContractResolver
        {
            // As of 7.0.1, Json.NET suggests using a static instance for "stateless" contract resolvers, for performance reasons.
            // http://www.newtonsoft.com/json/help/html/ContractResolver.htm
            // http://www.newtonsoft.com/json/help/html/M_Newtonsoft_Json_Serialization_DefaultContractResolver__ctor_1.htm
            // "Use the parameterless constructor and cache instances of the contract resolver within your application for optimal performance."

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var properties = base.CreateProperties(type, memberSerialization);
                if (properties != null)
                    return properties.OrderBy(p => p.DeclaringType.BaseTypesAndSelf().Count()).ToList();
                return properties;
            }
        }
    }

    public static class TypeExtensions
    {
        public static IEnumerable<Type> BaseTypesAndSelf(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }
}