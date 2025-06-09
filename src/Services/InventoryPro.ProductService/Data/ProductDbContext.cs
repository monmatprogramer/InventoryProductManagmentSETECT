using Microsoft.EntityFrameworkCore;

namespace InventoryPro.ProductService.Data
    {
    /// <summary>
    /// Database context for product service
    /// </summary>
    public class ProductDbContext : DbContext
        {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options)
            {
            }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            base.OnModelCreating(modelBuilder);

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.Property(e => e.Price).HasPrecision(10, 2);

                // Relationship with Category
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId);
            });

            // Configure StockMovement entity
            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.StockMovements)
                    .HasForeignKey(e => e.ProductId);
            });

            // Seed initial categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" },
                new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items" },
                new Category { Id = 3, Name = "Food & Beverages", Description = "Consumable products" },
                new Category { Id = 4, Name = "Home & Garden", Description = "Household items and garden supplies" },
                new Category { Id = 5, Name = "Sports & Outdoors", Description = "Sports equipment and outdoor gear" }
            );

            // Seed initial products
            modelBuilder.Entity<Product>().HasData(
                new Product
                    {
                    Id = 1,
                    Name = "Laptop Pro 15",
                    SKU = "LAP-001",
                    Description = "High-performance laptop with 15-inch display",
                    Price = 1299.99m,
                    StockQuantity = 50,
                    MinimumStock = 10,
                    CategoryId = 1,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                new Product
                    {
                    Id = 2,
                    Name = "Wireless Mouse",
                    SKU = "MOU-001",
                    Description = "Ergonomic wireless mouse with USB receiver",
                    Price = 29.99m,
                    StockQuantity = 200,
                    MinimumStock = 50,
                    CategoryId = 1,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    }
            );
            }
        }
    }