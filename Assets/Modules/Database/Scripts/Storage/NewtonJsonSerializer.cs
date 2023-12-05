using Newtonsoft.Json;

namespace GameDatabase.Storage
{
    public class NewtonJsonSerializer : IJsonSerializer
    {
        public NewtonJsonSerializer()
        {
            _settings = new JsonSerializerSettings
            {
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

        private readonly JsonSerializerSettings _settings;
    }
}
