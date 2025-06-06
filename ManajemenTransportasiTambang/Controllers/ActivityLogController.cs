using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManajemenTransportasiTambang.Data;
using ManajemenTransportasiTambang.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ManajemenTransportasiTambang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ActivityLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ActivityLog
        public async Task<IActionResult> Index(string searchString, string module, string dateFrom, string dateTo, int page = 1)
        {
            // Page size - number of items per page
            int pageSize = 15;
            
            // Query activity logs
            var query = _context.ActivityLogs
                .Include(a => a.Reservation)
                .AsQueryable();
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                string search = searchString.ToLower();
                query = query.Where(l => 
                    l.Username.ToLower().Contains(search) ||
                    l.Action.ToLower().Contains(search) ||
                    l.Module.ToLower().Contains(search) ||
                    (l.Details != null && l.Details.ToLower().Contains(search))
                );
                
                ViewData["CurrentFilter"] = searchString;
            }

            // Filter by module if specified
            if (!string.IsNullOrEmpty(module))
            {
                query = query.Where(l => l.Module == module);
                ViewData["CurrentModule"] = module;
            }
            
            // Filter by date range if provided
            DateTime? fromDate = null;
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out DateTime parsedFromDate))
            {
                fromDate = parsedFromDate.Date;
                query = query.Where(l => l.Timestamp >= fromDate);
                ViewData["CurrentDateFrom"] = dateFrom;
            }
            
            DateTime? toDate = null;
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out DateTime parsedToDate))
            {
                toDate = parsedToDate.Date.AddDays(1).AddSeconds(-1); // End of the day
                query = query.Where(l => l.Timestamp <= toDate);
                ViewData["CurrentDateTo"] = dateTo;
            }
            
            // Get unique modules for filter dropdown
            ViewData["Modules"] = await _context.ActivityLogs
                .Select(l => l.Module)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
            
            // Calculate total items and pages
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            
            // Get the current page of items, ordered by timestamp descending (newest first)
            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Pass pagination info to view
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            
            return View(logs);
        }

        // GET: ActivityLog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _context.ActivityLogs
                .Include(a => a.Reservation)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }
        
        // GET: ActivityLog/Report
        public async Task<IActionResult> Report(string module, string dateFrom, string dateTo)
        {
            // Query activity logs
            var query = _context.ActivityLogs
                .Include(a => a.Reservation)
                .AsQueryable();
                
            // Filter by module if specified
            if (!string.IsNullOrEmpty(module))
            {
                query = query.Where(l => l.Module == module);
                ViewData["CurrentModule"] = module;
            }
            
            // Filter by date range if provided
            DateTime? fromDate = null;
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out DateTime parsedFromDate))
            {
                fromDate = parsedFromDate.Date;
                query = query.Where(l => l.Timestamp >= fromDate);
                ViewData["CurrentDateFrom"] = dateFrom;
            }
            else
            {
                // Default to last 30 days if no date provided
                fromDate = DateTime.Now.AddDays(-30).Date;
                query = query.Where(l => l.Timestamp >= fromDate);
                ViewData["CurrentDateFrom"] = fromDate.Value.ToString("yyyy-MM-dd");
            }
            
            DateTime? toDate = null;
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out DateTime parsedToDate))
            {
                toDate = parsedToDate.Date.AddDays(1).AddSeconds(-1); // End of the day
                query = query.Where(l => l.Timestamp <= toDate);
                ViewData["CurrentDateTo"] = dateTo;
            }
            else
            {
                // Default to today if no date provided
                toDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1); // End of today
                ViewData["CurrentDateTo"] = DateTime.Now.ToString("yyyy-MM-dd");
            }
            
            // Get unique modules for filter dropdown
            ViewData["Modules"] = await _context.ActivityLogs
                .Select(l => l.Module)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
            
            // Get logs ordered by timestamp descending (newest first)
            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
                
            // Get module activity statistics
            var moduleStats = await _context.ActivityLogs
                .Where(l => l.Timestamp >= fromDate && l.Timestamp <= toDate)
                .GroupBy(l => l.Module)
                .Select(g => new { Module = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
                
            ViewData["ModuleStats"] = moduleStats;
            
            // Get user activity statistics
            var userStats = await _context.ActivityLogs
                .Where(l => l.Timestamp >= fromDate && l.Timestamp <= toDate)
                .GroupBy(l => l.Username)
                .Select(g => new { Username = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();
                
            ViewData["UserStats"] = userStats;
            
            // Get activity by date statistics
            var dateStats = await _context.ActivityLogs
                .Where(l => l.Timestamp >= fromDate && l.Timestamp <= toDate)
                .GroupBy(l => l.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();
                
            ViewData["DateStats"] = dateStats;
            
            return View(logs);
        }
    }
}
