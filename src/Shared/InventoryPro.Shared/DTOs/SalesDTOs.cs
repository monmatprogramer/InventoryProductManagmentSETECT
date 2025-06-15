using System.ComponentModel.DataAnnotations;

namespace InventoryPro.Shared.DTOs
    {
    /// <summary>
    /// Data Transfer Object for customer information
    /// </summary>
    public class CustomerDto
        {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone format")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Customer statistics
        public decimal TotalPurchases { get; set; }
        public int OrderCount { get; set; }
        public DateTime? LastOrderDate { get; set; }
        }

    /// <summary>
    /// Data Transfer Object for sales transactions
    /// </summary>
    public class SaleDto
        {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
        public List<SaleItemDto> Items { get; set; } = new();

        // Payment information
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }

        // Additional details
        public string Notes { get; set; } = string.Empty;
        public int UserId { get; set; } // Who processed the sale
        public string UserName { get; set; } = string.Empty;
        }

    /// <summary>
    /// Data Transfer Object for individual sale items
    /// </summary>
    public class SaleItemDto
        {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal FinalAmount => Subtotal - DiscountAmount;
        }

    /// <summary>
    /// Data Transfer Object for creating new sales
    /// </summary>
    public class CreateSaleDto
        {
        [Required(ErrorMessage = "Customer is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreateSaleItemDto> Items { get; set; } = new();

        public string PaymentMethod { get; set; } = "Cash";
        public decimal PaidAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        }

    /// <summary>
    /// Data Transfer Object for creating sale items
    /// </summary>
    public class CreateSaleItemDto
        {
        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product SKU is required")]
        [StringLength(50, ErrorMessage = "Product SKU cannot exceed 50 characters")]
        public string ProductSKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        public decimal DiscountAmount { get; set; } = 0;
        }
    }
