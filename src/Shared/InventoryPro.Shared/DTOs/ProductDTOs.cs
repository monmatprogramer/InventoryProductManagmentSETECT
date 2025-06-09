using System.ComponentModel.DataAnnotations;

namespace InventoryPro.Shared.DTOs
    {
    /// <summary>
    /// Data Transfer Object for product information
    /// Contains all product details for display and editing
    /// </summary>
    public class ProductDto
        {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string SKU { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Additional fields for inventory management
        public int MinStock { get; set; } = 10; // Minimum stock level for alerts
        public bool IsActive { get; set; } = true;
        public string? ImageUrl { get; set; }
        }

    /// <summary>
    /// Data Transfer Object for creating or updating products
    /// Contains only the fields that can be modified by users
    /// </summary>
    public class CreateProductDto
        {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string SKU { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public int MinStock { get; set; } = 10;
        public string? ImageUrl { get; set; }
        }

    /// <summary>
    /// Data Transfer Object for product categories
    /// </summary>
    public class CategoryDto
        {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public int ProductCount { get; set; } // Number of products in this category
        }
    }
