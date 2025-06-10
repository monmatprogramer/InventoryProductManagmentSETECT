using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryPro.SalesService.Models
    {
    /// <summary>
    /// Customer entity for sales transactions
    /// </summary>
    public class Customer
        {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        }

    /// <summary>
    /// Sale entity representing a sales transaction
    /// </summary>
    public class Sale
        {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [Required]
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled

        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash";

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ChangeAmount { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; } // User who processed the sale

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        }

    /// <summary>
    /// Sale item entity representing individual items in a sale
    /// </summary>
    public class SaleItem
        {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }

        public Sale? Sale { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ProductSKU { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [NotMapped]
        public decimal Subtotal => Quantity * UnitPrice;

        [NotMapped]
        public decimal FinalAmount => Subtotal - DiscountAmount;
        }

    /// <summary>
    /// Payment entity for tracking payment details
    /// </summary>
    public class Payment
        {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }

        public Sale? Sale { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        public string Status { get; set; } = "Completed"; // Completed, Pending, Failed

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        }
    }