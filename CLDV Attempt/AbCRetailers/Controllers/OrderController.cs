using Microsoft.AspNetCore.Mvc;
using AbCRetailers.Models;
using AbCRetailers.Services; 
using System;
using System.Threading.Tasks;
using AbCRetailers.Models.AbCRetailers.Models;

namespace AbCRetailers.Controllers
{
    public class OrderController : Controller
    {
        private readonly IAzureStorageService _storage;

        public OrderController(IAzureStorageService storage)
        {
            _storage = storage;
        }

        // GET: /Order
        public async Task<IActionResult> Index()
        {
            var list = await _storage.GetAllEntitiesAsync<Order>("Orders");
            return View(list);
        }

        // GET: /Order/Create
        public IActionResult Create() => View();

        // POST: /Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (!ModelState.IsValid)
                return View(order);

            order.PartitionKey = "Orders"; // MUST match what you're using for querying
            order.RowKey = Guid.NewGuid().ToString();
            order.OrderDateUtc = DateTime.Now;
            order.Status = OrderStatus.Pending;
            var totalPrice = order.Quantity * order.UnitPrice;
           

            await _storage.AddEntityAsync(order);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Order/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var order = await _storage.GetEntityAsync<Order>("Orders", id);
            return order == null ? NotFound() : View(order);
        }

        // GET: /Order/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var order = await _storage.GetEntityAsync<Order>("Orders", id);
            return order == null ? NotFound() : View(order);
        }

        // POST: /Order/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Order updated)
        {
            if (!ModelState.IsValid)
                return View(updated);

            var original = await _storage.GetEntityAsync<Order>("Orders", id);
            if (original == null)
                return NotFound();

            original.Status = updated.Status;
            await _storage.UpdateEntityAsync<Order>(original);
            

            return RedirectToAction(nameof(Index));
        }

        // GET: /Order/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            var order = await _storage.GetEntityAsync<Order>("Orders", id);
            return order == null ? NotFound() : View(order);
        }

        // POST: /Order/Delete (formerly DeleteConfirmed)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            await _storage.DeleteEntityAsync<Order>(partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
