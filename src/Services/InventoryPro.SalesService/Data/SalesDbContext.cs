using InventoryPro.SalesService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace InventoryPro.SalesService.Data
    {
    /// <summary>
    /// Database context for sales service
    /// </summary>
    public class SalesDbContext : DbContext
        {
        public SalesDbContext(DbContextOptions<SalesDbContext> options)
            : base(options)
            {
            }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            base.OnModelCreating(modelBuilder);

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Phone);

                // Relationship with Sales
                entity.HasMany(e => e.Sales)
                    .WithOne(s => s.Customer)
                    .HasForeignKey(s => s.CustomerId);
            });

            // Configure Sale entity
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
                entity.Property(e => e.PaidAmount).HasPrecision(10, 2);
                entity.Property(e => e.ChangeAmount).HasPrecision(10, 2);

                // Relationship with SaleItems
                entity.HasMany(e => e.SaleItems)
                    .WithOne(si => si.Sale)
                    .HasForeignKey(si => si.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SaleItem entity
            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
            });

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(10, 2);

                entity.HasOne(e => e.Sale)
                    .WithMany()
                    .HasForeignKey(e => e.SaleId);
            });

            // Seed initial customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                    {
                    Id = 1,
                    Name = "Walk-in Customer",
                    Email = "walkin@example.com",
                    Phone = "0000000000",
                    Address = "N/A",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                new Customer
                    {
                    Id = 2,
                    Name = "John Doe",
                    Email = "john.doe@example.com",
                    Phone = "1234567890",
                    Address = "123 Main St, City",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                new Customer
                    {
                    Id = 3,
                    Name = "Jane Smith",
                    Email = "jane.smith@example.com",
                    Phone = "0987654321",
                    Address = "456 Oak Ave, Town",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    }
            );
            }
        }
    }