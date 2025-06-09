using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryPro.ProductService.Models
{
    /// <summary>
    /// Product entity representing inventory items
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty; // Stock Keeping Unit

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public int MinimumStock { get; set; } = 10; // Alert threshold

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property for stock movements
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }

    /// <summary>
    /// Product category for organization
    /// </summary>
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    /// <summary>
    /// Tracks stock movements (in/out) for audit trail
    /// </summary>
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public int Quantity { get; set; } // Positive for stock in, negative for stock out

        [StringLength(50)]
        public string MovementType { get; set; } = string.Empty; // Purchase, Sale, Adjustment, Return

        [StringLength(200)]
        public string Reason { get; set; } = string.Empty;

        public DateTime MovementDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;
    }
}
