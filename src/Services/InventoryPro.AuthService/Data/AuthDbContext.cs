using Microsoft.EntityFrameworkCore;
using InventoryPro.AuthService.Models;

namespace InventoryPro.AuthService.Data
    {
    public class AuthDbContext : DbContext
        {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
            {
            }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // Seed initial admin user
                entity.HasData(new User
                    {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "$2a$11$rBNrVkNdKi8qVz7QWXJ9q.6L6U7LMX8J8V8YH5L5V9QXjZ6jVUq2a", // admin123
                    Email = "admin@inventorypro.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    });
            });
            }
        }
    }