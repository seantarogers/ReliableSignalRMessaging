namespace Contracts
{
    using Messages.Commands;

    public interface IBackOfficeHubClient
    {
        void InsertDocument(InsertDocumentIntoRemoteBackOfficeCommand insertDocumentIntoRemoteBackOfficeCommand);
    }
}