using Microsoft.AspNetCore.Mvc;
using AbCRetailers.Models;
using AbCRetailers.Services;

namespace AbCRetailers.Controllers
{
    public class ProductController : Controller
    {
        private const string BlobContainerName = "product-images";
        private readonly IAzureStorageService _storage;

        public ProductController(IAzureStorageService storage)
        {
            _storage = storage;
        }

        // GET: /Product
        public async Task<IActionResult> Index()
        {
            var products = await _storage.GetAllEntitiesAsync<Product>("Products");
            return View(products);
        }

        // GET: /Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Product/Create
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
                return View(product);

            if (imageFile?.Length > 0)
            {
                var url = await _storage.UploadBlobAsync(BlobContainerName, imageFile);
                product.ImageUrl = url;
            }

            await _storage.AddEntityAsync("Products", product);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Product/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var product = await _storage.GetEntityAsync<Product>("Products", id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: /Product/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(string id, Product updated, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
                return View(updated);

            var original = await _storage.GetEntityAsync<Product>("Products", id);
            if (original == null)
                return NotFound();

            original.ProductName = updated.ProductName;
            original.Price = updated.Price;
            original.Description = updated.Description;

            if (imageFile?.Length > 0)
            {
                original.ImageUrl = await _storage.UploadBlobAsync(BlobContainerName, imageFile);
            }

            await _storage.UpdateEntityAsync("Products", original);
            return RedirectToAction(nameof(Index));
        }
    }
}
