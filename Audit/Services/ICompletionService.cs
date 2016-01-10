namespace Audit.Services
{
    using System;

    public interface ICompletionService
    {
        void CompleteCorrelatedMessages(Guid correlationId);
    }
}