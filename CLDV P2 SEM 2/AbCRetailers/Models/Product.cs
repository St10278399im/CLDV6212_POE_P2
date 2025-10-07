using Azure;
using Azure.Data.Tables;
using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace AbCRetailers.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public string ProductId { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "Product Name")]
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100)]
        public string ProductName { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Price")]
        [Range(0.01, 999999, ErrorMessage = "Price must be greater than zero")]
        public double Price { get; set; }

        [Display(Name = "Stock Available")]
        [Range(0, 1000000, ErrorMessage = "Stock must be non-negative")]
        public int StockAvailable { get; set; }

        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
