namespace Persistence
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration;

    using Domain;

    public class HubConnectionLogConfiguration : EntityTypeConfiguration<HubConnectionLog>
    {
        public HubConnectionLogConfiguration()
        {
            ToTable("HubConnectionLog");
            HasKey(p => p.Id);
            Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(p => p.ConnectionEventType).IsRequired();
            Property(p => p.ConnectionId).IsRequired();
            Property(p => p.BrokerId).IsRequired();
            Property(p => p.CreateDate);
        }
    }
}