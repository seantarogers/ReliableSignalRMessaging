namespace Persistence
{
    using System.Data.Entity;

    using Domain;

    public class AuditContext : DbContext, IAuditContext
    {
        public AuditContext()
            : base("NServiceBus/Persistence")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<MessageLog> MessageLog { get; set; }

        public DbSet<HubConnectionLog> HubConnectionLog { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new MessageLogConfiguration());
            modelBuilder.Configurations.Add(new HubConnectionLogConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbContextTransaction BeginTransaction()
        {
            return Database.BeginTransaction();
        }
    }
}