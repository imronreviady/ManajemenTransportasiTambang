using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManajemenTransportasiTambang.Data;
using ManajemenTransportasiTambang.Models;
using ManajemenTransportasiTambang.Services;

namespace ManajemenTransportasiTambang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public DriverController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Driver
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1)
        {
            // Set sorting parameters and views
            ViewData["CurrentFilter"] = searchString;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["LicenseSortParam"] = sortOrder == "license" ? "license_desc" : "license";
            ViewData["AvailableSortParam"] = sortOrder == "available" ? "available_desc" : "available";
            
            // Page size
            int pageSize = 10;
            
            // Query drivers
            var drivers = _context.Drivers.AsQueryable();
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                drivers = drivers.Where(d => 
                    d.Name.ToLower().Contains(searchString) ||
                    (d.LicenseNumber != null && d.LicenseNumber.ToLower().Contains(searchString)) ||
                    (d.PhoneNumber != null && d.PhoneNumber.ToLower().Contains(searchString))
                );
            }
            
            // Apply sorting
            drivers = sortOrder switch
            {
                "name_desc" => drivers.OrderByDescending(d => d.Name),
                "license" => drivers.OrderBy(d => d.LicenseNumber),
                "license_desc" => drivers.OrderByDescending(d => d.LicenseNumber),
                "available" => drivers.OrderBy(d => d.IsAvailable),
                "available_desc" => drivers.OrderByDescending(d => d.IsAvailable),
                _ => drivers.OrderBy(d => d.Name)
            };

            // Calculate total items and pages
            int totalItems = await drivers.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            
            // Get the current page of items
            var pagedDrivers = await drivers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            // Store pagination data for the view
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            ViewData["HasPreviousPage"] = page > 1;
            ViewData["HasNextPage"] = page < totalPages;
            
            return View(pagedDrivers);
        }

        // GET: Driver/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == id);
                
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // GET: Driver/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Driver/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Driver driver)
        {
            if (ModelState.IsValid)
            {
                // Set audit properties
                driver.CreatedAt = DateTime.Now;
                driver.LastModified = DateTime.Now;
                driver.CreatedBy = User.Identity?.Name;
                driver.ModifiedBy = User.Identity?.Name;
                
                _context.Add(driver);
                await _context.SaveChangesAsync();
                
                var user = await _context.Users.FindAsync(User.Identity?.Name);
                await _logService.LogActivityAsync(
                    user?.Id ?? "System",
                    user?.UserName ?? "System",
                    "Create",
                    "Driver",
                    $"Created driver record for {driver.Name}",
                    driver.Id
                );
                
                TempData["SuccessMessage"] = "Driver created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            return View(driver);
        }

        // GET: Driver/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            
            if (driver == null)
            {
                return NotFound();
            }
            
            return View(driver);
        }

        // POST: Driver/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Driver driver)
        {
            if (id != driver.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing driver to preserve CreatedAt and CreatedBy
                    var existingDriver = await _context.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                    if (existingDriver != null)
                    {
                        // Preserve creation information
                        driver.CreatedAt = existingDriver.CreatedAt;
                        driver.CreatedBy = existingDriver.CreatedBy;
                    }
                    
                    // Update audit fields
                    driver.LastModified = DateTime.Now;
                    driver.ModifiedBy = User.Identity?.Name;
                    
                    _context.Update(driver);
                    await _context.SaveChangesAsync();
                    
                    var user = await _context.Users.FindAsync(User.Identity?.Name);
                    await _logService.LogActivityAsync(
                        user?.Id ?? "System",
                        user?.UserName ?? "System",
                        "Update",
                        "Driver",
                        $"Updated driver record for {driver.Name}",
                        driver.Id
                    );
                    
                    TempData["SuccessMessage"] = "Driver updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(driver.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(driver);
        }

        // GET: Driver/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == id);
                
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // POST: Driver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            
            if (driver == null)
            {
                return NotFound();
            }
            
            // Check if driver is used in reservations
            bool isUsedInReservations = await _context.VehicleReservations
                .AnyAsync(r => r.DriverId == id && 
                              (r.Status == ReservationStatus.Pending || 
                               r.Status == ReservationStatus.PartiallyApproved || 
                               r.Status == ReservationStatus.Approved));
                               
            if (isUsedInReservations)
            {
                TempData["ErrorMessage"] = "Cannot delete driver because they are assigned to active reservations.";
                return RedirectToAction(nameof(Delete), new { id = driver.Id });
            }
            
            // Instead of deleting, set as unavailable
            driver.IsAvailable = false;
            _context.Update(driver);
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FindAsync(User.Identity?.Name);
            await _logService.LogActivityAsync(
                user?.Id ?? "System",
                user?.UserName ?? "System",
                "Delete",
                "Driver",
                $"Deactivated driver record for {driver.Name}",
                driver.Id
            );
            
            TempData["SuccessMessage"] = "Driver deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Driver/ToggleAvailability/5
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            
            if (driver == null)
            {
                return NotFound();
            }
            
            driver.IsAvailable = !driver.IsAvailable;
            _context.Update(driver);
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FindAsync(User.Identity?.Name);
            string status = driver.IsAvailable ? "Available" : "Unavailable";
            await _logService.LogActivityAsync(
                user?.Id ?? "System",
                user?.UserName ?? "System",
                "Update",
                "Driver",
                $"Changed driver {driver.Name} status to {status}",
                driver.Id
            );
            
            TempData["SuccessMessage"] = $"Driver marked as {status.ToLower()} successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
}
