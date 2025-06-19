using InventoryPro.ReportService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace InventoryPro.ReportService.Data
{
    /// <summary>
    /// Database context for storing report metadata and view data
    /// </summary>
    public class ReportDbContext : DbContext
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
        {
        }

        public DbSet<ReportRecord> ReportRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ReportRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReportType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Format).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Parameters).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ViewData).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                
                entity.HasIndex(e => e.ReportType);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}