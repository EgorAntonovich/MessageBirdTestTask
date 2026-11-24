using Microsoft.EntityFrameworkCore;
using Vexcash.Sms.Api.Domain;

namespace Vexcash.Sms.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<SmsMessage> SmsMessages => Set<SmsMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sms");

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PhoneNumber).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.CreditNumber).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.CreditNumber).IsUnique();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasData(new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                PhoneNumber = "+31612345678",
                Name = "Amelia Hart",
                CreditNumber = "VC-10482",
                Amount = 480.00m,
                DueDate = new DateOnly(2026, 11, 28)
            });
        });

        modelBuilder.Entity<MessageTemplate>(entity =>
        {
            entity.ToTable("message_templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.Body).HasMaxLength(2_000).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasData(
                SeedTemplate("20000000-0000-0000-0000-000000000001", "Payment reminder", TemplateKind.Reminder,
                    "{CreditNumber}\n\nDear {Name},\n\nPlease pay the outstanding amount of {Amount} by {DueDate}.\n\nKind regards,\nVexcash"),
                SeedTemplate("20000000-0000-0000-0000-000000000002", "Payment demand", TemplateKind.PaymentDemand,
                    "{CreditNumber}\n\nDear {Name},\n\nDespite our reminder, {Amount} remains outstanding. Please make payment no later than {DueDate}.\n\nKind regards,\nVexcash"),
                SeedTemplate("20000000-0000-0000-0000-000000000003", "Contract termination", TemplateKind.ContractTermination,
                    "{CreditNumber}\n\nDear {Name},\n\nBecause the outstanding amount of {Amount} was not received by {DueDate}, we are terminating the credit agreement. Please contact us immediately.\n\nKind regards,\nVexcash")
            );
        });

        modelBuilder.Entity<SmsMessage>(entity =>
        {
            entity.ToTable("sms_messages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Recipient).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(2_000).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.ProviderMessageId).HasMaxLength(128);
            entity.Property(x => x.ProviderStatus).HasMaxLength(64);
            entity.Property(x => x.ProviderResponseJson).HasColumnType("text");
            entity.Property(x => x.ErrorMessage).HasMaxLength(1_000);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.ProviderMessageId);
            entity.HasOne(x => x.Customer).WithMany(x => x.Messages).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Template).WithMany(x => x.Messages).HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static object SeedTemplate(string id, string name, TemplateKind kind, string body) => new
    {
        Id = Guid.Parse(id),
        Name = name,
        Kind = kind,
        Body = body,
        UpdatedAt = new DateTimeOffset(2026, 11, 24, 0, 0, 0, TimeSpan.Zero)
    };
}
