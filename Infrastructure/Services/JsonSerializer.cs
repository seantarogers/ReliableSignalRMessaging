namespace Infrastructure.Services
{
    using System;
    using System.IO;

    using Messages;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class JsonSerializer : IJsonSerializer
    {
        public string Serialize<TMessage>(TMessage messageToSerialize) where TMessage : Message
        {

            return JsonConvert.SerializeObject(messageToSerialize);
        }

        public TMessage Deserialize<TMessage>(string json)
        {
            return JsonConvert.DeserializeObject<TMessage>(json);
        }
    }
}