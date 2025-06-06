using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ManajemenTransportasiTambang.Data;
using ManajemenTransportasiTambang.Models;
using ManajemenTransportasiTambang.Services;

namespace ManajemenTransportasiTambang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public VehicleController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Vehicle
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1)
        {
            // Set sorting parameters and views
            ViewData["CurrentFilter"] = searchString;
            ViewData["RegistrationSortParam"] = string.IsNullOrEmpty(sortOrder) ? "reg_desc" : "";
            ViewData["BrandSortParam"] = sortOrder == "brand" ? "brand_desc" : "brand";
            ViewData["TypeSortParam"] = sortOrder == "type" ? "type_desc" : "type";
            ViewData["ActiveSortParam"] = sortOrder == "active" ? "active_desc" : "active";
            
            // Page size
            int pageSize = 10;
            
            // Query vehicles with locations
            var vehicles = _context.Vehicles.Include(v => v.Location).AsQueryable();
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                vehicles = vehicles.Where(v => 
                    v.RegistrationNumber.ToLower().Contains(searchString) ||
                    v.Brand.ToLower().Contains(searchString) ||
                    v.Model.ToLower().Contains(searchString) ||
                    v.Type.ToString().ToLower().Contains(searchString)
                );
            }
            
            // Apply sorting
            vehicles = sortOrder switch
            {
                "reg_desc" => vehicles.OrderByDescending(v => v.RegistrationNumber),
                "brand" => vehicles.OrderBy(v => v.Brand),
                "brand_desc" => vehicles.OrderByDescending(v => v.Brand),
                "type" => vehicles.OrderBy(v => v.Type),
                "type_desc" => vehicles.OrderByDescending(v => v.Type),
                "active" => vehicles.OrderBy(v => v.IsActive),
                "active_desc" => vehicles.OrderByDescending(v => v.IsActive),
                _ => vehicles.OrderBy(v => v.RegistrationNumber)
            };

            // Calculate total items and pages
            int totalItems = await vehicles.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            
            // Get the current page of items
            var pagedVehicles = await vehicles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            // Store pagination data for the view
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            ViewData["HasPreviousPage"] = page > 1;
            ViewData["HasNextPage"] = page < totalPages;
            
            return View(pagedVehicles);
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Location)
                .Include(v => v.MaintenanceRecords)
                .Include(v => v.FuelConsumptionRecords)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicle/Create
        public IActionResult Create()
        {
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        // POST: Vehicle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                // Set audit properties
                vehicle.CreatedAt = DateTime.Now;
                vehicle.LastModified = DateTime.Now;
                vehicle.CreatedBy = User.Identity?.Name;
                vehicle.ModifiedBy = User.Identity?.Name;
                
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                
                var user = await _context.Users.FindAsync(User.Identity?.Name);
                await _logService.LogActivityAsync(
                    user?.Id ?? "System",
                    user?.UserName ?? "System",
                    "Create",
                    "Vehicle",
                    $"Created vehicle with registration number {vehicle.RegistrationNumber}",
                    vehicle.Id
                );
                
                TempData["SuccessMessage"] = "Vehicle created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", vehicle.LocationId);
            return View(vehicle);
        }

        // GET: Vehicle/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            
            if (vehicle == null)
            {
                return NotFound();
            }
            
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", vehicle.LocationId);
            return View(vehicle);
        }

        // POST: Vehicle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing vehicle to preserve CreatedAt and CreatedBy
                    var existingVehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
                    if (existingVehicle != null)
                    {
                        // Preserve creation information
                        vehicle.CreatedAt = existingVehicle.CreatedAt;
                        vehicle.CreatedBy = existingVehicle.CreatedBy;
                    }
                    
                    // Update audit fields
                    vehicle.LastModified = DateTime.Now;
                    vehicle.ModifiedBy = User.Identity?.Name;
                    
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                    
                    var user = await _context.Users.FindAsync(User.Identity?.Name);
                    await _logService.LogActivityAsync(
                        user?.Id ?? "System",
                        user?.UserName ?? "System",
                        "Update",
                        "Vehicle",
                        $"Updated vehicle with registration number {vehicle.RegistrationNumber}",
                        vehicle.Id
                    );
                    
                    TempData["SuccessMessage"] = "Vehicle updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.Id))
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
            
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", vehicle.LocationId);
            return View(vehicle);
        }

        // GET: Vehicle/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            
            if (vehicle == null)
            {
                return NotFound();
            }
            
            // Check if vehicle is used in reservations
            bool isUsedInReservations = await _context.VehicleReservations
                .AnyAsync(r => r.VehicleId == id && 
                              (r.Status == ReservationStatus.Pending || 
                               r.Status == ReservationStatus.PartiallyApproved || 
                               r.Status == ReservationStatus.Approved));
                               
            if (isUsedInReservations)
            {
                TempData["ErrorMessage"] = "Cannot delete vehicle because it is used in active reservations.";
                return RedirectToAction(nameof(Delete), new { id = vehicle.Id });
            }
            
            // Instead of deleting, set as inactive
            vehicle.IsActive = false;
            _context.Update(vehicle);
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FindAsync(User.Identity?.Name);
            await _logService.LogActivityAsync(
                user?.Id ?? "System",
                user?.UserName ?? "System",
                "Delete",
                "Vehicle",
                $"Deactivated vehicle with registration number {vehicle.RegistrationNumber}",
                vehicle.Id
            );
            
            TempData["SuccessMessage"] = "Vehicle deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Vehicle/ToggleStatus/5
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            
            if (vehicle == null)
            {
                return NotFound();
            }
            
            vehicle.IsActive = !vehicle.IsActive;
            _context.Update(vehicle);
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FindAsync(User.Identity?.Name);
            string action = vehicle.IsActive ? "Activated" : "Deactivated";
            await _logService.LogActivityAsync(
                user?.Id ?? "System",
                user?.UserName ?? "System",
                "Update",
                "Vehicle",
                $"{action} vehicle with registration number {vehicle.RegistrationNumber}",
                vehicle.Id
            );
            
            TempData["SuccessMessage"] = $"Vehicle {action.ToLower()} successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}
