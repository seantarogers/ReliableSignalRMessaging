namespace Persistence
{
    using System.Data.Entity;

    using Domain;

    public interface IAuditContext
    {
        int SaveChanges();

        DbSet<MessageLog> MessageLog { get; }
        DbSet<HubConnectionLog> HubConnectionLog { get; }

        DbContextTransaction BeginTransaction();
    }
}