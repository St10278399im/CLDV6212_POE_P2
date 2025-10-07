using Microsoft.AspNetCore.Mvc;
using AbCRetailers.Models;
using AbCRetailers.Services;

namespace AbCRetailers.Controllers
{
    public class UploadController : Controller
    {
        private readonly IAzureStorageService _storage;
        public UploadController(IAzureStorageService storage) => _storage = storage;

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (!ModelState.IsValid) return View(model);

            await _storage.UploadBlobAsync("proofs", model.ProofOfPayment, model.ProofOfPayment.FileName);
            return RedirectToAction(nameof(Index));
        }
    }
}
