using System.ComponentModel.DataAnnotations;

namespace InventoryPro.ReportService.Models
{
    /// <summary>
    /// Database model for storing report metadata and view data
    /// </summary>
    public class ReportRecord
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string ReportType { get; set; } = string.Empty; // Sales, Inventory, Financial, Custom
        
        [Required]
        [MaxLength(20)]
        public string Format { get; set; } = string.Empty; // View, PDF, Excel, CSV
        
        [MaxLength(200)]
        public string? Title { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // JSON serialized parameters used to generate the report
        public string? Parameters { get; set; }
        
        // JSON serialized view data (only stored for View format, not for file exports)
        public string? ViewData { get; set; }
        
        public int RecordCount { get; set; }
        public decimal? TotalAmount { get; set; }
        
        // Status tracking
        public string Status { get; set; } = "Generated"; // Generated, Failed, Expired
        public DateTime? ExpiresAt { get; set; }
        
        // File info (for exported reports)
        public string? FileName { get; set; }
        public long? FileSizeBytes { get; set; }
    }
}