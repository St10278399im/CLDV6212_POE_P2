using AbCRetailers.Models;
using AbCRetailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace AbCRetailers.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IAzureStorageService _storageService;

        public CustomerController(IAzureStorageService storageService)
        {
            _storageService = storageService;
        }

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _storageService.GetAllEntitiesAsync<Customer>("Customers");
                return View(customers);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading customers: {ex.Message}";
                return View(new List<Customer>());
            }
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            try
            {
                customer.PartitionKey = "Customers";
                customer.RowKey = Guid.NewGuid().ToString();

                await _storageService.AddEntityAsync(customer);
                TempData["SuccessMessage"] = "Customer created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating customer: {ex.Message}";
                return View(customer);
            }
        }

        // GET: Customer/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid customer ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var customer = await _storageService.GetEntityAsync<Customer>("Customers", id);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error retrieving customer: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customer/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Customer customer)
        {
            if (id != customer.RowKey)
            {
                TempData["ErrorMessage"] = "Customer ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
                return View(customer);

            try
            {
                await _storageService.UpdateEntityAsync(customer);
                TempData["SuccessMessage"] = "Customer updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating customer: {ex.Message}";
                return View(customer);
            }
        }

        // POST: Customer/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid customer ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var customer = await _storageService.GetEntityAsync<Customer>("Customers", id);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                await _storageService.DeleteEntityAsync<Customer>(customer.PartitionKey, customer.RowKey);
                TempData["SuccessMessage"] = "Customer deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting customer: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
