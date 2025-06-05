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
    public class FuelConsumptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public FuelConsumptionController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: FuelConsumption
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["DateSortParam"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["VehicleSortParam"] = sortOrder == "vehicle" ? "vehicle_desc" : "vehicle";
            ViewData["CostSortParam"] = sortOrder == "cost" ? "cost_desc" : "cost";
            
            // Page size
            int pageSize = 10;
            
            // Query fuel records with vehicle info
            var fuelRecords = _context.FuelConsumptionRecords
                .Include(f => f.Vehicle)
                .Include(f => f.Reservation)
                .AsQueryable();
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                fuelRecords = fuelRecords.Where(f => 
                    (f.Vehicle != null && f.Vehicle.RegistrationNumber.ToLower().Contains(searchString)) ||
                    (f.Notes != null && f.Notes.ToLower().Contains(searchString))
                );
            }
            
            // Apply sorting
            fuelRecords = sortOrder switch
            {
                "date_desc" => fuelRecords.OrderByDescending(f => f.FillDate),
                "vehicle" => fuelRecords.OrderBy(f => f.Vehicle.RegistrationNumber),
                "vehicle_desc" => fuelRecords.OrderByDescending(f => f.Vehicle.RegistrationNumber),
                "cost" => fuelRecords.OrderBy(f => f.Cost),
                "cost_desc" => fuelRecords.OrderByDescending(f => f.Cost),
                _ => fuelRecords.OrderBy(f => f.FillDate)
            };
            
            // Calculate total items and pages
            int totalItems = await fuelRecords.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            
            // Get the current page of items
            var pagedRecords = await fuelRecords
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

        // GET: FuelConsumption/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _context.FuelConsumptionRecords
                .Include(f => f.Vehicle)
                .Include(f => f.Reservation)
                .FirstOrDefaultAsync(f => f.Id == id);
                
            if (fuelRecord == null)
            {
                return NotFound();
            }

            return View(fuelRecord);
        }

        // GET: FuelConsumption/Create
        public IActionResult Create()
        {
            // Only show active vehicles
            ViewData["VehicleId"] = new SelectList(_context.Vehicles.Where(v => v.IsActive), "Id", "RegistrationNumber");
            
            // Show active reservations for optional linking
            ViewData["ReservationId"] = new SelectList(
                _context.VehicleReservations
                    .Where(r => r.Status == ReservationStatus.Approved || r.Status == ReservationStatus.PartiallyApproved)
                    .OrderByDescending(r => r.RequestDate), 
                "Id", 
                "Purpose"
            );
            
            return View();
        }

        // POST: FuelConsumption/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FuelConsumptionRecord fuelRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fuelRecord);
                await _context.SaveChangesAsync();
                
                var user = await _context.Users.FindAsync(User.Identity?.Name);
                await _logService.LogActivityAsync(
                    user?.Id ?? "System",
                    user?.UserName ?? "System",
                    "Create",
                    "FuelConsumptionRecord",
                    $"Created fuel record for vehicle ID {fuelRecord.VehicleId} - {fuelRecord.Quantity}L at cost {fuelRecord.Cost:C}",
                    fuelRecord.Id
                );
                
                TempData["SuccessMessage"] = "Fuel consumption record created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["VehicleId"] = new SelectList(_context.Vehicles.Where(v => v.IsActive), "Id", "RegistrationNumber", fuelRecord.VehicleId);
            ViewData["ReservationId"] = new SelectList(
                _context.VehicleReservations
                    .Where(r => r.Status == ReservationStatus.Approved || r.Status == ReservationStatus.PartiallyApproved)
                    .OrderByDescending(r => r.RequestDate), 
                "Id", 
                "Purpose", 
                fuelRecord.ReservationId
            );
            
            return View(fuelRecord);
        }

        // GET: FuelConsumption/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _context.FuelConsumptionRecords.FindAsync(id);
            
            if (fuelRecord == null)
            {
                return NotFound();
            }
            
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber", fuelRecord.VehicleId);
            ViewData["ReservationId"] = new SelectList(
                _context.VehicleReservations
                    .OrderByDescending(r => r.RequestDate), 
                "Id", 
                "Purpose", 
                fuelRecord.ReservationId
            );
            
            return View(fuelRecord);
        }

        // POST: FuelConsumption/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FuelConsumptionRecord fuelRecord)
        {
            if (id != fuelRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fuelRecord);
                    await _context.SaveChangesAsync();
                    
                    var user = await _context.Users.FindAsync(User.Identity?.Name);
                    await _logService.LogActivityAsync(
                        user?.Id ?? "System",
                        user?.UserName ?? "System",
                        "Update",
                        "FuelConsumptionRecord",
                        $"Updated fuel record ID {fuelRecord.Id} for vehicle ID {fuelRecord.VehicleId}",
                        fuelRecord.Id
                    );
                    
                    TempData["SuccessMessage"] = "Fuel consumption record updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FuelRecordExists(fuelRecord.Id))
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
            
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber", fuelRecord.VehicleId);
            ViewData["ReservationId"] = new SelectList(
                _context.VehicleReservations
                    .OrderByDescending(r => r.RequestDate), 
                "Id", 
                "Purpose", 
                fuelRecord.ReservationId
            );
            
            return View(fuelRecord);
        }

        // GET: FuelConsumption/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _context.FuelConsumptionRecords
                .Include(f => f.Vehicle)
                .Include(f => f.Reservation)
                .FirstOrDefaultAsync(f => f.Id == id);
                
            if (fuelRecord == null)
            {
                return NotFound();
            }

            return View(fuelRecord);
        }

        // POST: FuelConsumption/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fuelRecord = await _context.FuelConsumptionRecords.FindAsync(id);
            
            if (fuelRecord == null)
            {
                return NotFound();
            }
            
            _context.FuelConsumptionRecords.Remove(fuelRecord);
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FindAsync(User.Identity?.Name);
            await _logService.LogActivityAsync(
                user?.Id ?? "System",
                user?.UserName ?? "System",
                "Delete",
                "FuelConsumptionRecord",
                $"Deleted fuel record ID {id} for vehicle ID {fuelRecord.VehicleId}",
                fuelRecord.VehicleId
            );
            
            TempData["SuccessMessage"] = "Fuel consumption record deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        // GET: FuelConsumption/Report
        public async Task<IActionResult> Report()
        {
            // Get all vehicles for the dropdown
            ViewData["Vehicles"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber");
            
            // Default date range is current month
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            ViewData["StartDate"] = firstDayOfMonth.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = lastDayOfMonth.ToString("yyyy-MM-dd");
            
            return View();
        }
        
        // POST: FuelConsumption/GenerateReport
        [HttpPost]
        public async Task<IActionResult> GenerateReport(DateTime startDate, DateTime endDate, int? vehicleId)
        {
            // Query for fuel records in the date range
            var query = _context.FuelConsumptionRecords
                .Include(f => f.Vehicle)
                .Where(f => f.FillDate >= startDate && f.FillDate <= endDate);
            
            // Filter by vehicle if specified
            if (vehicleId.HasValue)
            {
                query = query.Where(f => f.VehicleId == vehicleId.Value);
            }
            
            // Get records ordered by date
            var fuelRecords = await query.OrderBy(f => f.FillDate).ToListAsync();
            
            // Calculate summary data
            var summary = new
            {
                TotalCost = fuelRecords.Sum(f => f.Cost),
                TotalQuantity = fuelRecords.Sum(f => f.Quantity),
                AverageCostPerLiter = fuelRecords.Any() ? fuelRecords.Sum(f => f.Cost) / fuelRecords.Sum(f => f.Quantity) : 0,
                RecordsCount = fuelRecords.Count,
                VehiclesSummary = fuelRecords
                    .GroupBy(f => f.VehicleId)
                    .Select(g => new
                    {
                        VehicleId = g.Key,
                        RegistrationNumber = g.First().Vehicle?.RegistrationNumber,
                        TotalQuantity = g.Sum(r => r.Quantity),
                        TotalCost = g.Sum(r => r.Cost),
                        FillCount = g.Count()
                    })
                    .ToList()
            };
            
            ViewData["Summary"] = summary;
            ViewData["StartDate"] = startDate.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate.ToString("yyyy-MM-dd");
            ViewData["SelectedVehicleId"] = vehicleId;
            
            ViewData["Vehicles"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber", vehicleId);
            
            return View("Report", fuelRecords);
        }

        private bool FuelRecordExists(int id)
        {
            return _context.FuelConsumptionRecords.Any(e => e.Id == id);
        }
    }
}
