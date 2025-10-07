using Microsoft.AspNetCore.Mvc;
using AbCRetailers.Models.ViewModels;
using AbCRetailers.Services;
using AbCRetailers.Models;
using AbCRetailers.Models.AbCRetailers.Models;

namespace AbCRetailers.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAzureStorageService _storage;
        public HomeController(IAzureStorageService storage) => _storage = storage;

        public async Task<IActionResult> Index()
        {
            var cust = await _storage.GetAllEntitiesAsync<Customer>("Customers");
            var prod = await _storage.GetAllEntitiesAsync<Product>("Products");
            var ord = await _storage.GetAllEntitiesAsync<Order>("Orders");

            var vm = new HomeViewModel
            {
                CustomerCount = cust.Count,
                ProductCount = prod.Count,
                OrderCount = ord.Count,
                FeaturedProducts = prod.Take(5).ToList()
            };
            return View(vm);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
    }
}
