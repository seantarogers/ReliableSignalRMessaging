namespace MessagingInfrastructure.Services
{
    using Messages;

    public interface IJsonSerializer
    {
        string Serialize<TMessage>(TMessage messageToSerialize) where TMessage : Message;

        TMessage Deserialize<TMessage>(string json);
    }
}