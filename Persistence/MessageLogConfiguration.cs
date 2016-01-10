namespace Persistence
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration;

    using Domain;

    public class MessageLogConfiguration : EntityTypeConfiguration<MessageLog>
    {
        public MessageLogConfiguration()
        {
            ToTable("MessageLog");
            HasKey(p => p.Id);
            Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(p => p.MessageId).IsRequired();
            Property(p => p.CorrelationId).IsRequired();
            Property(p => p.CreateDate).IsRequired();
            Property(p => p.CompletionDate);
            Property(p => p.Body).IsRequired();
            Property(p => p.MessageType).IsRequired();
            
        }
    }
}