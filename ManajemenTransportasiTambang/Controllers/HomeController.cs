using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ManajemenTransportasiTambang.Models;
using ManajemenTransportasiTambang.Services;

namespace ManajemenTransportasiTambang.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IVehicleReservationService _reservationService;

    public HomeController(
        ILogger<HomeController> logger,
        IVehicleReservationService reservationService)
    {
        _logger = logger;
        _reservationService = reservationService;
    }

    public async Task<IActionResult> Index()
    {
        // Dashboard data for vehicle usage statistics
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today.AddDays(30);
        
        var reservationStats = await _reservationService.GetReservationStatisticsAsync(startDate, endDate);
        
        // Data for the chart - past 30 days and next 30 days
        var labels = new List<string>();
        var data = new List<int>();
        
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            labels.Add(date.ToString("MMM dd"));
            data.Add(reservationStats.ContainsKey(date) ? reservationStats[date] : 0);
        }
        
        // Get pending reservations that need approval
        var pendingReservations = await _reservationService.GetReservationsByStatusAsync(ReservationStatus.Pending);
        var partiallyApprovedReservations = await _reservationService.GetReservationsByStatusAsync(ReservationStatus.PartiallyApproved);
        
        ViewData["ChartLabels"] = labels;
        ViewData["ChartData"] = data;
        ViewData["PendingReservations"] = pendingReservations;
        ViewData["PartiallyApprovedReservations"] = partiallyApprovedReservations;
        
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
}