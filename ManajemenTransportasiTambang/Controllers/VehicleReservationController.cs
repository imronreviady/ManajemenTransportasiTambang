using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ManajemenTransportasiTambang.Models;
using ManajemenTransportasiTambang.Services;
using ManajemenTransportasiTambang.Data;

namespace ManajemenTransportasiTambang.Controllers;

[Authorize]
public class VehicleReservationController : Controller
{
    private readonly IVehicleReservationService _reservationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogService _logService;
    private readonly ApplicationDbContext _context;

    public VehicleReservationController(
        IVehicleReservationService reservationService,
        UserManager<ApplicationUser> userManager,
        ILogService logService,
        ApplicationDbContext context)
    {
        _reservationService = reservationService;
        _userManager = userManager;
        _logService = logService;
        _context = context;
    }

    // GET: VehicleReservation
    public async Task<IActionResult> Index(string searchString, int page = 1)
    {
        // Page size - number of items per page
        int pageSize = 10;
        
        var user = await _userManager.GetUserAsync(User);
        IEnumerable<VehicleReservation> reservations;

        // Check if the user is an admin (has role Admin)
        if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
        {
            // Admins see all reservations
            reservations = await _reservationService.GetAllReservationsAsync();
        }
        else
        {
            // Regular users see their own reservations
            reservations = user != null 
                ? await _reservationService.GetReservationsByRequesterAsync(user.Id)
                : Enumerable.Empty<VehicleReservation>();
        }

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower();
            reservations = reservations.Where(r => 
                r.Purpose.ToLower().Contains(searchString) ||
                (r.Requester?.FullName?.ToLower().Contains(searchString) ?? false) ||
                (r.Vehicle?.RegistrationNumber?.ToLower().Contains(searchString) ?? false) ||
                (r.Driver?.Name?.ToLower().Contains(searchString) ?? false) ||
                r.Id.ToString().Contains(searchString)
            );

            ViewData["CurrentFilter"] = searchString;
        }

        // Calculate total items and pages
        int totalItems = reservations.Count();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        // Ensure page is within valid range
        page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
        
        // Get the current page of items
        var pagedReservations = reservations
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
            
        // Store pagination data for the view
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalItems"] = totalItems;
        ViewData["HasPreviousPage"] = page > 1;
        ViewData["HasNextPage"] = page < totalPages;

        return View(pagedReservations);
    }

    // GET: VehicleReservation/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        
        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }

    // GET: VehicleReservation/Create
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        // Get list of vehicles, drivers, and approvers for the form
        ViewData["Vehicles"] = new SelectList(_context.Vehicles.Where(v => v.IsActive), "Id", "RegistrationNumber");
        ViewData["Drivers"] = new SelectList(_context.Drivers.Where(d => d.IsAvailable), "Id", "Name");
        
        // For approvers, we need users who can approve (not those with approval level 0)
        ViewData["Approvers"] = new SelectList(
            await _userManager.GetUsersInRoleAsync("Approver"), 
            "Id", 
            "FullName");

        return View();
    }

    // POST: VehicleReservation/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(VehicleReservation reservation, List<string> approverIds)
    {
        if (ModelState.IsValid)
        {
            // Check vehicle availability
            bool isVehicleAvailable = await _reservationService.CheckVehicleAvailabilityAsync(
                reservation.VehicleId, reservation.StartDate, reservation.EndDate);
                
            if (!isVehicleAvailable)
            {
                ModelState.AddModelError("VehicleId", "The selected vehicle is not available for the specified dates.");
                await PrepareCreateViewData();
                return View(reservation);
            }

            // Check driver availability
            bool isDriverAvailable = await _reservationService.CheckDriverAvailabilityAsync(
                reservation.DriverId, reservation.StartDate, reservation.EndDate);
                
            if (!isDriverAvailable)
            {
                ModelState.AddModelError("DriverId", "The selected driver is not available for the specified dates.");
                await PrepareCreateViewData();
                return View(reservation);
            }

            // Set the requester ID and audit properties
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                reservation.RequesterId = user.Id;
                
                // Set audit properties
                reservation.CreatedAt = DateTime.Now;
                reservation.LastModified = DateTime.Now;
                reservation.CreatedBy = user.UserName;
                reservation.ModifiedBy = user.UserName;

                // Create the reservation with approvers
                await _reservationService.CreateReservationAsync(reservation, approverIds);

                return RedirectToAction(nameof(Index));
            }
            
            ModelState.AddModelError(string.Empty, "User not found.");
        }

        await PrepareCreateViewData();
        return View(reservation);

        // Local async function to prepare ViewData
        async Task PrepareCreateViewData()
        {
            ViewData["Vehicles"] = new SelectList(_context.Vehicles.Where(v => v.IsActive), "Id", "RegistrationNumber");
            ViewData["Drivers"] = new SelectList(_context.Drivers.Where(d => d.IsAvailable), "Id", "Name");
            ViewData["Approvers"] = new SelectList(
                await _userManager.GetUsersInRoleAsync("Approver"), 
                "Id", 
                "FullName");
        }
    }

    // GET: VehicleReservation/Approve/5
    [Authorize(Roles = "Approver")]
    public async Task<IActionResult> Approve(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        
        if (reservation == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found");
        }
        
        // Find the approval that belongs to this user
        var approval = reservation.Approvals?.FirstOrDefault(a => a.ApproverId == user.Id);
        
        if (approval == null)
        {
            return NotFound("You are not assigned to approve this reservation.");
        }

        // Check if all previous levels are approved before allowing this level to approve
        if (approval.ApprovalLevel > 1)
        {
            // Check all previous approval levels
            for (int i = 1; i < approval.ApprovalLevel; i++)
            {
                var previousApproval = reservation.Approvals?.FirstOrDefault(a => a.ApprovalLevel == i);
                if (previousApproval == null || previousApproval.Status != ApprovalStatus.Approved)
                {
                    TempData["ErrorMessage"] = $"Level {i} approval is required before you can approve this request.";
                    return RedirectToAction(nameof(MyApprovals));
                }
            }
        }

        ViewData["ReservationId"] = id;
        ViewData["ApprovalId"] = approval.Id;
        
        return View();
    }

    // POST: VehicleReservation/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Approver")]
    public async Task<IActionResult> Approve(int approvalId, ApprovalStatus status, string? comments)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found");
        }
        
        // Process the approval
        var result = await _reservationService.ProcessApprovalAsync(approvalId, user.Id, status, comments);
        
        if (!result)
        {
            TempData["ErrorMessage"] = "Failed to process approval. Please ensure you are authorized and try again.";
        }
        else
        {
            TempData["SuccessMessage"] = "Approval processed successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: VehicleReservation/Cancel/5
    public async Task<IActionResult> Cancel(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        
        if (reservation == null)
        {
            return NotFound();
        }

        // Ensure the user is either the requester or an admin
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found");
        }
        
        bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        
        if (reservation.RequesterId != user.Id && !isAdmin)
        {
            return Forbid();
        }

        return View(reservation);
    }

    // POST: VehicleReservation/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? cancellationReason)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        
        if (reservation == null)
        {
            return NotFound();
        }

        // Ensure the user is either the requester or an admin
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found");
        }
        
        bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        
        if (reservation.RequesterId != user.Id && !isAdmin)
        {
            return Forbid();
        }

        await _reservationService.UpdateReservationStatusAsync(id, ReservationStatus.Cancelled, cancellationReason);

        return RedirectToAction(nameof(Index));
    }

    // GET: VehicleReservation/Complete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Complete(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        
        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }

    // POST: VehicleReservation/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CompleteConfirmed(int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        await _reservationService.UpdateReservationStatusAsync(id, ReservationStatus.Completed);

        return RedirectToAction(nameof(Index));
    }

    // GET: VehicleReservation/Report
    [Authorize(Roles = "Admin")]
    public IActionResult Report()
    {
        // Default to current month
        var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, DateTimeKind.Local);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        ViewData["StartDate"] = startDate.ToString("yyyy-MM-dd");
        ViewData["EndDate"] = endDate.ToString("yyyy-MM-dd");
        ViewData["Statuses"] = new SelectList(Enum.GetValues(typeof(ReservationStatus)));
        
        return View();
    }

    // POST: VehicleReservation/ExportReport
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportReport(DateTime startDate, DateTime endDate, ReservationStatus? status)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var excelData = await _reservationService.ExportReservationsToExcelAsync(startDate, endDate, status);
        
        string fileName = $"VehicleReservations_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.xlsx";
        
        // Log the export
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            await _logService.LogActivityAsync(
                user.Id,
                user.UserName!,
                "ExportReport",
                "VehicleReservation",
                $"Exported reservations report from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
            );
        }
        
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // GET: VehicleReservation/MyApprovals
    [Authorize(Roles = "Approver")]
    public async Task<IActionResult> MyApprovals()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found");
        }
        
        // Get all reservations that need approval from this user
        var pendingApprovals = await _context.ReservationApprovals
            .Where(a => a.ApproverId == user.Id && a.Status == ApprovalStatus.Pending)
            .Include(a => a.Reservation)
            .ThenInclude(r => r!.Vehicle)
            .Include(a => a.Reservation)
            .ThenInclude(r => r!.Requester)
            .Include(a => a.Reservation)
            .ThenInclude(r => r!.Driver)
            .ToListAsync();
            
        return View(pendingApprovals);
    }
}
