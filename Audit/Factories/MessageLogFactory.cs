namespace Audit.Factories
{
    using System.Collections.Generic;
    using System.Text;

    using Domain;
    
    using Messages;

    using MessagingInfrastructure.Services;

    public class MessageLogFactory : IMessageLogFactory
    {
        private readonly IJsonSerializer jsonSerializer;

        public MessageLogFactory(IJsonSerializer jsonSerializer)
        {
            this.jsonSerializer = jsonSerializer;
        }

        public MessageLog Create(Message message, IDictionary<string, string> headers)
        {
            var stringBuilder = ExtractHeaders(headers);
            
            return new MessageLog
                       {
                           MessageId = message.Id,
                           CorrelationId = message.CorrelationId,
                           CreateDate = message.CreateDate,
                           Body = jsonSerializer.Serialize(message),
                           MessageType = message.ToString(),
                           Headers = stringBuilder.ToString(),
                           BrokerId = message.BrokerId
                       };
        }

        private static StringBuilder ExtractHeaders(IDictionary<string, string> headers)
        {
            var stringBuilder = new StringBuilder();
            foreach (var header in headers)
            {
                stringBuilder.Append(header.Key + "=" + header.Value + "; ");
            }
            return stringBuilder;
        }
    }
}