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
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public LocationController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Location
        public async Task<IActionResult> Index(string searchString, int? regionId, int page = 1)
        {
            // Page size - number of items per page
            int pageSize = 10;
            
            // Query locations with regions
            var locations = _context.Locations
                .Include(l => l.Region)
                .AsQueryable();
            
            // Apply region filter if provided
            if (regionId.HasValue)
            {
                locations = locations.Where(l => l.RegionId == regionId.Value);
                ViewData["CurrentRegionId"] = regionId.Value;
                
                var region = await _context.Regions.FindAsync(regionId.Value);
                if (region != null)
                {
                    ViewData["CurrentRegionName"] = region.Name;
                }
            }
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                locations = locations.Where(l => 
                    l.Name.ToLower().Contains(searchString) ||
                    (l.Address != null && l.Address.ToLower().Contains(searchString)) ||
                    (l.Region != null && l.Region.Name.ToLower().Contains(searchString))
                );
                
                ViewData["CurrentFilter"] = searchString;
            }
            
            // Calculate total items and pages
            int totalItems = await locations.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            
            // Get the current page of items
            var pagedLocations = await locations
                .OrderBy(l => l.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Store pagination data for the view
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            ViewData["HasPreviousPage"] = page > 1;
            ViewData["HasNextPage"] = page < totalPages;

            // Prepare regions for dropdown filter
            ViewData["Regions"] = new SelectList(_context.Regions, "Id", "Name", regionId);
            
            return View(pagedLocations);
        }

        // GET: Location/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .Include(l => l.Region)
                .Include(l => l.AssignedVehicles)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // GET: Location/Create
        public IActionResult Create(int? regionId)
        {
            // Auto-select region if provided
            var location = new Location();
            if (regionId.HasValue)
            {
                location.RegionId = regionId.Value;
            }
            
            // Prepare dropdowns
            ViewData["RegionId"] = new SelectList(_context.Regions, "Id", "Name", location.RegionId);
            ViewData["LocationTypes"] = new SelectList(Enum.GetValues(typeof(LocationType)));

            return View(location);
        }

        // POST: Location/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,Type,RegionId")] Location location)
        {
            if (ModelState.IsValid)
            {
                _context.Add(location);
                await _context.SaveChangesAsync();
                
                var user = await _context.Users.FindAsync(User.Identity?.Name);
                await _logService.LogActivityAsync(
                    user?.Id ?? "System",
                    user?.UserName ?? "System",
                    "Create",
                    "Location",
                    $"Created location '{location.Name}'",
                    location.Id
                );
                
                TempData["SuccessMessage"] = "Location created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            // Prepare dropdowns
            ViewData["RegionId"] = new SelectList(_context.Regions, "Id", "Name", location.RegionId);
            ViewData["LocationTypes"] = new SelectList(Enum.GetValues(typeof(LocationType)));

            return View(location);
        }

        // GET: Location/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            
            // Prepare dropdowns
            ViewData["RegionId"] = new SelectList(_context.Regions, "Id", "Name", location.RegionId);
            ViewData["LocationTypes"] = new SelectList(Enum.GetValues(typeof(LocationType)));
            
            return View(location);
        }

        // POST: Location/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Type,RegionId")] Location location)
        {
            if (id != location.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                    
                    var user = await _context.Users.FindAsync(User.Identity?.Name);
                    await _logService.LogActivityAsync(
                        user?.Id ?? "System",
                        user?.UserName ?? "System",
                        "Update",
                        "Location",
                        $"Updated location '{location.Name}'",
                        location.Id
                    );
                    
                    TempData["SuccessMessage"] = "Location updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.Id))
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
            
            // Prepare dropdowns
            ViewData["RegionId"] = new SelectList(_context.Regions, "Id", "Name", location.RegionId);
            ViewData["LocationTypes"] = new SelectList(Enum.GetValues(typeof(LocationType)));

            return View(location);
        }

        // GET: Location/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .Include(l => l.Region)
                .Include(l => l.AssignedVehicles)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // POST: Location/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _context.Locations
                .Include(l => l.AssignedVehicles)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (location == null)
            {
                return NotFound();
            }
            
            // Check if location has assigned vehicles
            if (location.AssignedVehicles != null && location.AssignedVehicles.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete location '{location.Name}' because it has {location.AssignedVehicles.Count} vehicle(s) assigned to it. Please reassign these vehicles first.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            
            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FindAsync(User.Identity?.Name);
            await _logService.LogActivityAsync(
                user?.Id ?? "System",
                user?.UserName ?? "System",
                "Delete",
                "Location",
                $"Deleted location '{location.Name}'",
                id
            );
            
            TempData["SuccessMessage"] = "Location deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }
    }
}

