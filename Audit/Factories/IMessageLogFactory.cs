namespace Audit.Factories
{
    using System.Collections.Generic;

    using Domain;

    using Messages;

    public interface IMessageLogFactory
    {
        MessageLog Create(Message message, IDictionary<string, string> headers);
    }
}