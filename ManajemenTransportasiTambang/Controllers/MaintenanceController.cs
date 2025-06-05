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
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public MaintenanceController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Maintenance
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["DateSortParam"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["VehicleSortParam"] = sortOrder == "vehicle" ? "vehicle_desc" : "vehicle";
            ViewData["CostSortParam"] = sortOrder == "cost" ? "cost_desc" : "cost";
            
            // Page size
            int pageSize = 10;
            
            // Query maintenance records with vehicle info
            var maintenanceRecords = _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .AsQueryable();
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                maintenanceRecords = maintenanceRecords.Where(m => 
                    m.Description.ToLower().Contains(searchString) ||
                    (m.Vehicle != null && m.Vehicle.RegistrationNumber.ToLower().Contains(searchString)) ||
                    (m.ServiceProvider != null && m.ServiceProvider.ToLower().Contains(searchString))
                );
            }
            
            // Apply sorting
            maintenanceRecords = sortOrder switch
            {
                "date_desc" => maintenanceRecords.OrderByDescending(m => m.ServiceDate),
                "vehicle" => maintenanceRecords.OrderBy(m => m.Vehicle.RegistrationNumber),
                "vehicle_desc" => maintenanceRecords.OrderByDescending(m => m.Vehicle.RegistrationNumber),
                "cost" => maintenanceRecords.OrderBy(m => m.Cost),
                "cost_desc" => maintenanceRecords.OrderByDescending(m => m.Cost),
                _ => maintenanceRecords.OrderBy(m => m.ServiceDate)
            };
            
            // Calculate total items and pages
            int totalItems = await maintenanceRecords.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            
            // Get the current page of items
            var pagedRecords = await maintenanceRecords
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            // Store pagination data for the view
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            ViewData["HasPreviousPage"] = page > 1;
            ViewData["HasNextPage"] = page < totalPages;
            
            return View(pagedRecords);
        }

        // GET: Maintenance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceRecord = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (maintenanceRecord == null)
            {
                return NotFound();
            }

            return View(maintenanceRecord);
        }

        // GET: Maintenance/Create
        public IActionResult Create()
        {
            ViewData["VehicleId"] = new SelectList(_context.Vehicles.Where(v => v.IsActive), "Id", "RegistrationNumber");
            return View();
        }

        // POST: Maintenance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceRecord maintenanceRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(maintenanceRecord);
                await _context.SaveChangesAsync();
                
                var user = await _context.Users.FindAsync(User.Identity?.Name);
                await _logService.LogActivityAsync(
                    user?.Id ?? "System",
                    user?.UserName ?? "System",
                    "Create",
                    "MaintenanceRecord",
                    $"Created maintenance record for vehicle ID {maintenanceRecord.VehicleId} on {maintenanceRecord.ServiceDate:yyyy-MM-dd}",
                    maintenanceRecord.Id
                );
                
                // Update NextServiceDueDate on vehicle if the new service date is later than the last service date
                var vehicle = await _context.Vehicles.FindAsync(maintenanceRecord.VehicleId);
                if (vehicle != null && maintenanceRecord.ServiceDate > vehicle.LastServiceDate)
                {
                    vehicle.LastServiceDate = maintenanceRecord.ServiceDate;
                    
                    // Calculate next service due date (add 3 months by default)
                    vehicle.NextServiceDueDate = maintenanceRecord.ServiceDate.AddMonths(3);
                    
                    await _context.SaveChangesAsync();
                }
                
                TempData["SuccessMessage"] = "Maintenance record created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["VehicleId"] = new SelectList(_context.Vehicles.Where(v => v.IsActive), "Id", "RegistrationNumber", maintenanceRecord.VehicleId);
            return View(maintenanceRecord);
        }

        // GET: Maintenance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceRecord = await _context.MaintenanceRecords.FindAsync(id);
            
            if (maintenanceRecord == null)
            {
                return NotFound();
            }
            
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber", maintenanceRecord.VehicleId);
            return View(maintenanceRecord);
        }

        // POST: Maintenance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceRecord maintenanceRecord)
        {
            if (id != maintenanceRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(maintenanceRecord);
                    await _context.SaveChangesAsync();
                    
                    var user = await _context.Users.FindAsync(User.Identity?.Name);
                    await _logService.LogActivityAsync(
                        user?.Id ?? "System",
                        user?.UserName ?? "System",
                        "Update",
                        "MaintenanceRecord",
                        $"Updated maintenance record ID {maintenanceRecord.Id} for vehicle ID {maintenanceRecord.VehicleId}",
                        maintenanceRecord.Id
                    );
                    
                    // Check if we need to update the vehicle's last service date and next due date
                    var vehicle = await _context.Vehicles.FindAsync(maintenanceRecord.VehicleId);
                    if (vehicle != null)
                    {
                        // Get all maintenance records for this vehicle
                        var latestMaintenanceDate = await _context.MaintenanceRecords
                            .Where(m => m.VehicleId == maintenanceRecord.VehicleId)
                            .MaxAsync(m => m.ServiceDate);
                            
                        if (latestMaintenanceDate > vehicle.LastServiceDate)
                        {
                            vehicle.LastServiceDate = latestMaintenanceDate;
                            vehicle.NextServiceDueDate = latestMaintenanceDate.AddMonths(3);
                            await _context.SaveChangesAsync();
                        }
                    }
                    
                    TempData["SuccessMessage"] = "Maintenance record updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceRecordExists(maintenanceRecord.Id))
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
            
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber", maintenanceRecord.VehicleId);
            return View(maintenanceRecord);
        }

        // GET: Maintenance/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceRecord = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (maintenanceRecord == null)
            {
                return NotFound();
            }

            return View(maintenanceRecord);
        }

        // POST: Maintenance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maintenanceRecord = await _context.MaintenanceRecords.FindAsync(id);
            
            if (maintenanceRecord == null)
            {
                return NotFound();
            }
            
            _context.MaintenanceRecords.Remove(maintenanceRecord);
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FindAsync(User.Identity?.Name);
            await _logService.LogActivityAsync(
                user?.Id ?? "System",
                user?.UserName ?? "System",
                "Delete",
                "MaintenanceRecord",
                $"Deleted maintenance record ID {id} for vehicle ID {maintenanceRecord.VehicleId}",
                maintenanceRecord.VehicleId
            );
            
            // After deleting a record, we may need to update the last service date on the vehicle
            var vehicle = await _context.Vehicles.FindAsync(maintenanceRecord.VehicleId);
            if (vehicle != null)
            {
                var latestMaintenance = await _context.MaintenanceRecords
                    .Where(m => m.VehicleId == maintenanceRecord.VehicleId)
                    .OrderByDescending(m => m.ServiceDate)
                    .FirstOrDefaultAsync();
                    
                if (latestMaintenance != null)
                {
                    vehicle.LastServiceDate = latestMaintenance.ServiceDate;
                    vehicle.NextServiceDueDate = latestMaintenance.ServiceDate.AddMonths(3);
                }
                
                await _context.SaveChangesAsync();
            }
            
            TempData["SuccessMessage"] = "Maintenance record deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool MaintenanceRecordExists(int id)
        {
            return _context.MaintenanceRecords.Any(e => e.Id == id);
        }
    }
}
