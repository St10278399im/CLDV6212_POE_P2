using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace AbCRetailers.Models
{
    namespace AbCRetailers.Models
    {
        public enum OrderStatus
        {
            Submitted,
            Pending,
            Processing,
            Shipped,
            Delivered,
            Cancelled
        }

        public class Order : ITableEntity
        {
            public string PartitionKey { get; set; } = "Order";
            public string RowKey { get; set; } = Guid.NewGuid().ToString();

            [Display(Name = "Customer ID")]
            [Required(ErrorMessage = "Customer is required")]
            public string CustomerId { get; set; }

            [Display(Name = "Product ID")]
            [Required(ErrorMessage = "Product is required")]
            public string ProductId { get; set; }

            [Display(Name = "Product Name")]
            public string ProductName { get; set; }

            [Display(Name = "Quantity")]
            [Range(1, 1000, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }

            [Display(Name = "Unit Price")]
            [Range(0.01, 1000000)]
            public decimal UnitPrice { get; set; }

            [Display(Name = "Order Date")]
            [DataType(DataType.Date)]
            public DateTimeOffset OrderDateUtc { get; set; } = DateTimeOffset.UtcNow;

            [Display(Name = "Status")]
            [Required]
            public OrderStatus Status { get; set; } = OrderStatus.Submitted;

            // Optional UI convenience properties
            [Display(Name = "Customer Username")]
            public string? Username { get; set; }

            [Display(Name = "Total Price")]
            public decimal TotalPrice => UnitPrice * Quantity;

            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }

            // Computed ID used by client API
            public string Id => RowKey;
        }
    }
}
