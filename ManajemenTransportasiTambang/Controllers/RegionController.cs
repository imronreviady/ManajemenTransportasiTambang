using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManajemenTransportasiTambang.Data;
using ManajemenTransportasiTambang.Models;
using ManajemenTransportasiTambang.Services;

namespace ManajemenTransportasiTambang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RegionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public RegionController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Region
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            // Page size - number of items per page
            int pageSize = 10;
            
            // Query regions
            var regions = _context.Regions.AsQueryable();
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                regions = regions.Where(r => 
                    r.Name.ToLower().Contains(searchString) ||
                    (r.Description != null && r.Description.ToLower().Contains(searchString))
                );
                
                ViewData["CurrentFilter"] = searchString;
            }
            
            // Calculate total items and pages
            int totalItems = await regions.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            
            // Get the current page of items
            var pagedRegions = await regions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Store pagination data for the view
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            ViewData["HasPreviousPage"] = page > 1;
            ViewData["HasNextPage"] = page < totalPages;
            
            return View(pagedRegions);
        }

        // GET: Region/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions
                .Include(r => r.Locations)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (region == null)
            {
                return NotFound();
            }

            return View(region);
        }

        // GET: Region/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Region/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Region region)
        {
            if (ModelState.IsValid)
            {
                _context.Add(region);
                await _context.SaveChangesAsync();
                
                var user = await _context.Users.FindAsync(User.Identity?.Name);
                await _logService.LogActivityAsync(
                    user?.Id ?? "System",
                    user?.UserName ?? "System",
                    "Create",
                    "Region",
                    $"Created region '{region.Name}'",
                    region.Id
                );
                
                TempData["SuccessMessage"] = "Region created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(region);
        }

        // GET: Region/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions.FindAsync(id);
            if (region == null)
            {
                return NotFound();
            }
            
            return View(region);
        }

        // POST: Region/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Region region)
        {
            if (id != region.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(region);
                    await _context.SaveChangesAsync();
                    
                    var user = await _context.Users.FindAsync(User.Identity?.Name);
                    await _logService.LogActivityAsync(
                        user?.Id ?? "System",
                        user?.UserName ?? "System",
                        "Update",
                        "Region",
                        $"Updated region '{region.Name}'",
                        region.Id
                    );
                    
                    TempData["SuccessMessage"] = "Region updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegionExists(region.Id))
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

            return View(region);
        }

        // GET: Region/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions
                .Include(r => r.Locations)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (region == null)
            {
                return NotFound();
            }

            return View(region);
        }

        // POST: Region/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var region = await _context.Regions
                .Include(r => r.Locations)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (region == null)
            {
                return NotFound();
            }
            
            // Check if region has locations
            if (region.Locations != null && region.Locations.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete region '{region.Name}' because it has {region.Locations.Count} location(s) assigned to it. Please reassign or delete these locations first.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            
            _context.Regions.Remove(region);
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FindAsync(User.Identity?.Name);
            await _logService.LogActivityAsync(
                user?.Id ?? "System",
                user?.UserName ?? "System",
                "Delete",
                "Region",
                $"Deleted region '{region.Name}'",
                id
            );
            
            TempData["SuccessMessage"] = "Region deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool RegionExists(int id)
        {
            return _context.Regions.Any(e => e.Id == id);
        }
    }
}

