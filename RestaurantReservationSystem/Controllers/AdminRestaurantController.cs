using Microsoft.AspNetCore.Hosting;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Models;
using RestaurantReservationSystem.Services;
using Microsoft.AspNetCore.Hosting;

namespace RestaurantReservationSystem.Controllers
{
    public class AdminRestaurantController : Controller
    {
        private readonly FileUploadService _fileUploadService;
        private readonly RestaurantReservationContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Inject the DbContext and IWebHostEnvironment into the controller
        public AdminRestaurantController(FileUploadService fileUploadService, RestaurantReservationContext context, IWebHostEnvironment webHostEnvironment)
        {
            _fileUploadService = fileUploadService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Restaurants
        public async Task<IActionResult> Index()
        {
            var restaurants = await _context.Restaurants.ToListAsync();
            return View(restaurants);
        }

        // GET: Restaurants/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Restaurants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,PhoneNumber,Email,CuisineType,PriceRange,LogoUrl")] Restaurant restaurant, IFormFile logoFile)
        {
            if (ModelState.IsValid)
            {
                // Handle logo upload
                if (logoFile != null)
                {
                    const long maxFileSize = 2 * 1024 * 1024; // 2MB
                    if (logoFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("LogoFile", "File size cannot exceed 2MB");
                        return View(restaurant);
                    }
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", logoFile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(stream);
                    }
                    restaurant.LogoUrl = "/images/" + logoFile.FileName;
                }

                restaurant.CreatedAt = DateTime.Now;
                _context.Add(restaurant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Restaurant added successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // GET: Restaurants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(m => m.RestaurantId == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // GET: /AdminRestaurant/Edit/5
        public IActionResult Edit(int id)
        {
            var restaurant = _context.Restaurants.Find(id);
            if (restaurant == null)
            {
                return NotFound();
            }
            return View(restaurant);
        }

        // POST: /AdminRestaurant/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm]Restaurant restaurant, IFormFile logoFile)
        {
            if (id != restaurant.RestaurantId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Find the existing restaurant
                    var existingRestaurant = await _context.Restaurants.FindAsync(id);
                    if (existingRestaurant == null)
                    {
                        return NotFound();
                    }

                    // update the existing restaurant
                    existingRestaurant.Name = restaurant.Name;
                    existingRestaurant.Address = restaurant.Address;
                    existingRestaurant.PhoneNumber = restaurant.PhoneNumber;
                    existingRestaurant.Email = restaurant.Email;
                    existingRestaurant.CuisineType = restaurant.CuisineType;
                    existingRestaurant.PriceRange = restaurant.PriceRange;

                    // upload logoFile
                    if (logoFile != null)
                    {
                        try
                        {
                            string logoUrl = await _fileUploadService.UploadLogoToBlobAsync(logoFile);
                            existingRestaurant.LogoUrl = logoUrl; // update LogoUrl
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Blob Upload Error: " + ex.Message);
                            ModelState.AddModelError("", "Failed to upload logo. Please try again.");
                            return View(existingRestaurant);
                        }
                    }
                    // update the restaurant
                    _context.Update(restaurant);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Restaurant details saved successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Cannot update restaurant. Try again, and if the problem persists, contact support.");
                }
            }
            return View(restaurant);
        }
    
        // POST: Restaurants/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult Delete(int id)
        {
            // find the restaurant by id in db
            var restaurant = _context.Restaurants.FirstOrDefault(r => r.RestaurantId == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            // delete the logo file from Blob storage
            if (!string.IsNullOrEmpty(restaurant.LogoUrl))
            {
                // get the connection string from environment variables
                string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING") ?? string.Empty;
                if (string.IsNullOrEmpty(connectionString))
                {
                    // when connection string is not found
                    return StatusCode(500, "Blob storage connection string not found in environment variables.");
                }

                // delete the blob file
                string containerName = "team04container"; 
                string blobName = Path.GetFileName(restaurant.LogoUrl); // get the blob name from the URL
                
                // get a reference to the Blob Service Client
                var blobServiceClient = new BlobServiceClient(connectionString); 
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // get a reference to the Blob Client
                var blobClient = blobContainerClient.GetBlobClient(blobName);
                if (blobClient.Exists())
                {
                    blobClient.DeleteIfExists(); // delete the blob file
                }
            }

            // delete the restaurant record in the database
            _context.Restaurants.Remove(restaurant);
            _context.SaveChanges();

            // return success message
            TempData["SuccessMessage"] = "Restaurant deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
                /*
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                string filePath = Path.Combine(uploadsFolder, logoFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    logoFile.CopyTo(stream);
                }
                restaurant.LogoUrl = "/images/" + logoFile.FileName;
            }

            _context.Update(restaurant);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, contact support.");
        }
    }
    return View(restaurant);
}*/

