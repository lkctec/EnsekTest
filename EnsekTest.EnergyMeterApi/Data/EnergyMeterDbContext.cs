using Microsoft.EntityFrameworkCore;
using EnergyMeterApi.Models;

namespace EnergyMeterApi.Data;

public class EnergyMeterDbContext : DbContext
{
    public EnergyMeterDbContext(DbContextOptions<EnergyMeterDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<MeterReading> MeterReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.AccountId).ValueGeneratedNever(); // Disable identity/auto-increment - would not normally do this but we are inserting our own IDs from CSV
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<MeterReading>(entity =>
        {

            entity.ToTable("MeterReadings", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_MeterReadValue_NumericOnly", "MeterReadValue NOT LIKE '%[^0-9]%'");
            });

            entity.HasKey(e => e.Id);
            entity.Property(e => e.MeterReadingDateTime).IsRequired();
            entity.Property(e => e.MeterReadValue).IsRequired().HasMaxLength(5);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne<Account>()
                  .WithMany()
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}