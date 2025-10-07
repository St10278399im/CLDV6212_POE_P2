
using System.ComponentModel.DataAnnotations;
using AbCRetailers.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AbCRetailers.Models.ViewModels
{
    public class CreateViewModel
    {
        [Required]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public string ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        // Populated by controller for dropdowns
        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> Products { get; set; } = new();
    }
}
