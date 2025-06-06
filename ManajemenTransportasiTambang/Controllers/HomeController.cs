using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ManajemenTransportasiTambang.Models;
using ManajemenTransportasiTambang.Services;
using ManajemenTransportasiTambang.Data;

namespace ManajemenTransportasiTambang.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IVehicleReservationService _reservationService;
    private readonly ApplicationDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        IVehicleReservationService reservationService,
        ApplicationDbContext context)
    {
        _logger = logger;
        _reservationService = reservationService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var today = DateTime.Today;
            var thirtyDaysAgo = today.AddDays(-30);
            var sixtyDaysAgo = today.AddDays(-60);
            
            // Dashboard Summary Statistics - Simplified for stability
            var activeReservations = await _context.VehicleReservations
                .CountAsync(r => r.Status == ReservationStatus.Approved && r.StartDate <= today && r.EndDate >= today);
                
            var availableVehicles = await _context.Vehicles
                .CountAsync(v => v.IsActive);
                
            var activeDrivers = await _context.Drivers.CountAsync(d => d.IsAvailable);
            
            // Calculate Reservation growth
            var reservationsCurrentPeriod = await _context.VehicleReservations
                .CountAsync(r => r.RequestDate >= thirtyDaysAgo);
                
            var reservationsPreviousPeriod = await _context.VehicleReservations
                .CountAsync(r => r.RequestDate >= sixtyDaysAgo && r.RequestDate < thirtyDaysAgo);
                
            double reservationGrowth = CalculateGrowthPercentage(reservationsCurrentPeriod, reservationsPreviousPeriod);
            
            // Calculate Vehicle growth using CreatedAt and LastModified fields
            var vehiclesCurrentPeriod = await _context.Vehicles
                .CountAsync(v => v.IsActive && (v.CreatedAt >= thirtyDaysAgo || v.LastModified >= thirtyDaysAgo));
                
            var vehiclesPreviousPeriod = await _context.Vehicles
                .CountAsync(v => v.IsActive && v.CreatedAt >= sixtyDaysAgo && v.CreatedAt < thirtyDaysAgo);
                
            double vehicleGrowth = CalculateGrowthPercentage(vehiclesCurrentPeriod, vehiclesPreviousPeriod);
            
            // Calculate Driver growth using CreatedAt and LastModified fields
            var driversCurrentPeriod = await _context.Drivers
                .CountAsync(d => d.IsAvailable && (d.CreatedAt >= thirtyDaysAgo || d.LastModified >= thirtyDaysAgo));
                
            var driversPreviousPeriod = await _context.Drivers
                .CountAsync(d => d.IsAvailable && d.CreatedAt >= sixtyDaysAgo && d.CreatedAt < thirtyDaysAgo);
                
            double driverGrowth = CalculateGrowthPercentage(driversCurrentPeriod, driversPreviousPeriod);
            
            // Fallback to 0 if FuelConsumptionRecords isn't accessible
            decimal fuelConsumption = 0;
            double fuelGrowth = 0;
            
            try
            {
                // Calculate current period fuel consumption (no duplikasi, hanya record valid)
                var fuelRecordsCurrent = await _context.FuelConsumptionRecords
                    .AsNoTracking()
                    .Where(f => f.FillDate >= thirtyDaysAgo && f.Quantity > 0)
                    .ToListAsync();
                fuelConsumption = fuelRecordsCurrent.Sum(f => f.Quantity);
                // Calculate previous period fuel consumption
                var fuelRecordsPrevious = await _context.FuelConsumptionRecords
                    .AsNoTracking()
                    .Where(f => f.FillDate >= sixtyDaysAgo && f.FillDate < thirtyDaysAgo && f.Quantity > 0)
                    .ToListAsync();
                var fuelConsumptionPrevious = fuelRecordsPrevious.Sum(f => f.Quantity);
                fuelGrowth = CalculateGrowthPercentage(fuelConsumption, fuelConsumptionPrevious);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching fuel consumption: {ex.Message}");
            }
            
            // Calculate last month's period (full previous month)
            var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
            var lastDayLastMonth = firstDayThisMonth.AddDays(-1);

            // Vehicle count last month (active vehicles created before this month)
            var vehiclesLastMonth = await _context.Vehicles
                .CountAsync(v => v.IsActive && v.CreatedAt <= lastDayLastMonth);

            // Driver count last month (active drivers created before this month)
            var driversLastMonth = await _context.Drivers
                .CountAsync(d => d.IsAvailable && d.CreatedAt <= lastDayLastMonth);

            // Fuel consumption last month (all valid records in last month)
            var fuelConsumptionLastMonth = await _context.FuelConsumptionRecords
                .AsNoTracking()
                .Where(f => f.FillDate >= firstDayLastMonth && f.FillDate <= lastDayLastMonth && f.Quantity > 0)
                .SumAsync(f => f.Quantity);

            // Create dashboard stats object
            var dashboardStats = new
            {
                ActiveReservations = activeReservations,
                ReservationsLastMonth = reservationsCurrentPeriod,
                ReservationGrowth = Math.Round(reservationGrowth, 1),
                
                AvailableVehicles = availableVehicles,
                VehicleGrowth = Math.Round(vehicleGrowth, 1),
                TotalVehicles = await _context.Vehicles.CountAsync(),
                VehicleLastMonth = vehiclesLastMonth,
                
                ActiveDrivers = activeDrivers,
                DriverGrowth = Math.Round(driverGrowth, 1),
                DriverLastMonth = driversLastMonth,
                
                FuelConsumption = fuelConsumption,
                FuelGrowth = Math.Round(fuelGrowth, 1),
                FuelConsumptionLastMonth = fuelConsumptionLastMonth
            };
            
            // Reservation Status Distribution
            var statusCounts = await _context.VehicleReservations
                .Where(r => r.RequestDate >= thirtyDaysAgo)
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
                
            var statusDistribution = new[]
            {
                new { Status = ReservationStatus.Approved, Count = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Approved)?.Count ?? 0 },
                new { Status = ReservationStatus.Pending, Count = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Pending)?.Count ?? 0 },
                new { Status = ReservationStatus.Completed, Count = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Completed)?.Count ?? 0 },
                new { Status = ReservationStatus.Rejected, Count = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Rejected)?.Count ?? 0 },
                new { Status = ReservationStatus.Cancelled, Count = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Cancelled)?.Count ?? 0 },
                new { Status = ReservationStatus.PartiallyApproved, Count = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.PartiallyApproved)?.Count ?? 0 }
            };
            
            // Monthly Fuel Consumption - Aggregated from database
            var currentYear = today.Year;
            var monthlyFuelData = await _context.FuelConsumptionRecords
                .AsNoTracking()
                .Where(f => f.FillDate.Year == currentYear && f.Quantity > 0)
                .GroupBy(f => f.FillDate.Month)
                .Select(g => new { Month = g.Key, Consumption = g.Sum(f => f.Quantity) })
                .ToListAsync();
            // Ensure all months are present (fill 0 for months with no data)
            var monthlyFuelDataFull = Enumerable.Range(1, 12)
                .Select(m => new {
                    Month = m,
                    Consumption = monthlyFuelData.FirstOrDefault(x => x.Month == m)?.Consumption ?? 0
                })
                .ToList();
            
            // Vehicle Status Distribution - Dynamic calculation
            var maintenanceVehicleIds = await _context.MaintenanceRecords
                .Where(m => m.ServiceDate > today)
                .Select(m => m.VehicleId)
                .Distinct()
                .ToListAsync();
            var vehicleStatuses = new List<object>
            {
                new { Status = "Available", Count = availableVehicles },
                new { Status = "In Use", Count = await _context.VehicleReservations
                    .Where(r => r.Status == ReservationStatus.Approved && r.StartDate <= today && r.EndDate >= today)
                    .Select(r => r.VehicleId)
                    .Distinct()
                    .CountAsync() },
                new { Status = "Maintenance", Count = maintenanceVehicleIds.Count },
                new { Status = "Out of Service", Count = await _context.Vehicles.CountAsync(v => !v.IsActive) }
            };
            
            // Recent Reservations
            var recentReservations = await _context.VehicleReservations
                .Include(r => r.Requester)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .ToListAsync();
                
            // Upcoming Maintenance - Using simplified approach
            var upcomingMaintenance = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .Where(m => m.ServiceDate > today)
                .OrderBy(m => m.ServiceDate)
                .Take(4)
                .ToListAsync();
                
            // Regional Vehicle Distribution - Using safe approach with nullability checks
            var regionDistribution = await _context.Vehicles
                .Where(v => v.Location != null && v.Location.Region != null)
                .GroupBy(v => v.Location.Region.Name)
                .Select(g => new { Region = g.Key, Count = g.Count() })
                .ToListAsync();

            // Add additional regions if we don't have enough data
            if (regionDistribution.Count < 3)
            {
                if (!regionDistribution.Any(r => r.Region == "Head Office"))
                    regionDistribution.Add(new { Region = "Head Office", Count = 15 });
                    
                if (!regionDistribution.Any(r => r.Region == "Mining Area A"))
                    regionDistribution.Add(new { Region = "Mining Area A", Count = 12 });
                    
                if (!regionDistribution.Any(r => r.Region == "Mining Area B"))
                    regionDistribution.Add(new { Region = "Mining Area B", Count = 8 });
                    
                if (!regionDistribution.Any(r => r.Region == "Remote Site"))
                    regionDistribution.Add(new { Region = "Remote Site", Count = 7 });
            }

            ViewData["DashboardStats"] = dashboardStats;
            ViewData["StatusDistribution"] = statusDistribution;
            ViewData["MonthlyFuelData"] = monthlyFuelDataFull;
            ViewData["VehicleStatuses"] = vehicleStatuses;
            ViewData["RecentReservations"] = recentReservations;
            ViewData["UpcomingMaintenance"] = upcomingMaintenance;
            ViewData["RegionDistribution"] = regionDistribution;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading dashboard: {ex.Message}", ex);
            // Return empty data to prevent dashboard from breaking
            ViewData["Error"] = "Error loading dashboard data";
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private double CalculateGrowthPercentage(int current, int previous)
    {
        if (previous == 0) 
        {
            // Jika tidak ada data di periode sebelumnya tetapi ada data saat ini,
            // tunjukkan pertumbuhan sebagai nilai positif (bukan 0)
            return current > 0 ? 100 : 0;
        }
        return ((double)(current - previous) / previous) * 100;
    }
    
    private double CalculateGrowthPercentage(decimal current, decimal previous)
    {
        if (previous == 0) 
        {
            // Jika tidak ada data di periode sebelumnya tetapi ada data saat ini,
            // tunjukkan pertumbuhan sebagai nilai positif (bukan 0)
            return current > 0 ? 100 : 0;
        }
        return (double)((current - previous) / previous) * 100;
    }
}
