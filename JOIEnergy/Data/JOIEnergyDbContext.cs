using Microsoft.EntityFrameworkCore;
using JOIEnergy.Domain;
using System;

namespace JOIEnergy.Data
{
    public class JOIEnergyDbContext : DbContext
    {
        public JOIEnergyDbContext(DbContextOptions<JOIEnergyDbContext> options) : base(options)
        {
        }

        public DbSet<ElectricityReadingEntity> ElectricityReadings { get; set; }
        public DbSet<PricePlanEntity> PricePlans { get; set; }
        public DbSet<SmartMeterAccount> SmartMeterAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ElectricityReadings table
            modelBuilder.Entity<ElectricityReadingEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SmartMeterId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reading).HasColumnType("decimal(18,6)");
                entity.Property(e => e.Time).IsRequired();
                
                entity.HasIndex(e => new { e.SmartMeterId, e.Time });
            });

            // PricePlans table
            modelBuilder.Entity<PricePlanEntity>(entity =>
            {
                entity.HasKey(e => e.PlanName);
                entity.Property(e => e.PlanName).HasMaxLength(50);
                entity.Property(e => e.UnitRate).HasColumnType("decimal(18,6)");
            });

            // SmartMeterAccounts table (for smart meter to price plan mapping)
            modelBuilder.Entity<SmartMeterAccount>(entity =>
            {
                entity.HasKey(e => e.SmartMeterId);
                entity.Property(e => e.SmartMeterId).HasMaxLength(50);
                entity.Property(e => e.PricePlanId).HasMaxLength(50);
            });
        }
    }

    // Simple entity classes for database
    public class ElectricityReadingEntity
    {
        public int Id { get; set; }
        public string SmartMeterId { get; set; } = string.Empty;
        public decimal Reading { get; set; }
        public DateTime Time { get; set; }
    }

    public class PricePlanEntity
    {
        public string PlanName { get; set; } = string.Empty;
        public int EnergySupplier { get; set; }
        public decimal UnitRate { get; set; }
    }

    public class SmartMeterAccount
    {
        public string SmartMeterId { get; set; } = string.Empty;
        public string PricePlanId { get; set; } = string.Empty;
    }
}