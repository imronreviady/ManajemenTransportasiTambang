using ManajemenTransportasiTambang.Data;
using ManajemenTransportasiTambang.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
// Hapus referensi EPPlus
// using OfficeOpenXml;
// using OfficeOpenXml.Style;
// Tambahkan referensi ClosedXML
using ClosedXML.Excel;
using System.IO;

namespace ManajemenTransportasiTambang.Services;

public class VehicleReservationService : IVehicleReservationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogService _logService;

    public VehicleReservationService(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<IEnumerable<VehicleReservation>> GetAllReservationsAsync()
    {
        return await _context.VehicleReservations
            .Include(r => r.Requester)
            .Include(r => r.Vehicle)
            .Include(r => r.Driver)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
    }

    public async Task<VehicleReservation?> GetReservationByIdAsync(int id)
    {
        return await _context.VehicleReservations
            .Include(r => r.Requester)
            .Include(r => r.Vehicle)
            .Include(r => r.Driver)
            .Include(r => r.Approvals)
                .ThenInclude(a => a.Approver)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<VehicleReservation>> GetReservationsByRequesterAsync(string requesterId)
    {
        return await _context.VehicleReservations
            .Include(r => r.Vehicle)
            .Include(r => r.Driver)
            .Where(r => r.RequesterId == requesterId)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<VehicleReservation>> GetReservationsByStatusAsync(ReservationStatus status)
    {
        return await _context.VehicleReservations
            .Include(r => r.Requester)
            .Include(r => r.Vehicle)
            .Include(r => r.Driver)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<VehicleReservation>> GetReservationsByVehicleAsync(int vehicleId)
    {
        return await _context.VehicleReservations
            .Include(r => r.Requester)
            .Include(r => r.Driver)
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<VehicleReservation>> GetReservationsByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.VehicleReservations
            .Include(r => r.Requester)
            .Include(r => r.Vehicle)
            .Include(r => r.Driver)
            .Where(r => (r.StartDate >= start && r.StartDate <= end) || 
                        (r.EndDate >= start && r.EndDate <= end) ||
                        (r.StartDate <= start && r.EndDate >= end))
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReservationApproval>> GetApprovalsByReservationIdAsync(int reservationId)
    {
        return await _context.ReservationApprovals
            .Include(a => a.Approver)
            .Where(a => a.ReservationId == reservationId)
            .OrderBy(a => a.ApprovalLevel)
            .ToListAsync();
    }

    public async Task<VehicleReservation> CreateReservationAsync(VehicleReservation reservation, List<string> approverIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Set initial status
            reservation.Status = ReservationStatus.Pending;
            
            // Add the reservation
            _context.VehicleReservations.Add(reservation);
            await _context.SaveChangesAsync();
            
            // Create approval records for each approver
            int level = 1;
            foreach (string approverId in approverIds)
            {
                var approval = new ReservationApproval
                {
                    ReservationId = reservation.Id,
                    ApproverId = approverId,
                    Status = ApprovalStatus.Pending,
                    ApprovalLevel = level++
                };
                
                _context.ReservationApprovals.Add(approval);
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            // Log the creation
            var user = await _context.Users.FindAsync(reservation.RequesterId);
            await _logService.LogActivityAsync(
                reservation.RequesterId, 
                user?.UserName ?? "Unknown",
                "Create",
                "VehicleReservation",
                $"Created vehicle reservation from {reservation.StartDate:yyyy-MM-dd} to {reservation.EndDate:yyyy-MM-dd} for vehicle ID {reservation.VehicleId}",
                reservation.Id
            );
            
            return reservation;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateReservationStatusAsync(int reservationId, ReservationStatus status, string? cancellationReason = null)
    {
        var reservation = await _context.VehicleReservations.FindAsync(reservationId);
        
        if (reservation == null)
        {
            throw new ArgumentException("Reservation not found", nameof(reservationId));
        }
        
        reservation.Status = status;
        
        if (status == ReservationStatus.Cancelled && !string.IsNullOrEmpty(cancellationReason))
        {
            reservation.CancellationReason = cancellationReason;
        }
        
        await _context.SaveChangesAsync();
        
        var requester = await _context.Users.FindAsync(reservation.RequesterId);
        
        // Log the status update
        await _logService.LogActivityAsync(
            requester?.Id ?? "System",
            requester?.UserName ?? "System",
            "Update",
            "VehicleReservation",
            $"Updated reservation status to {status} for reservation ID {reservationId}",
            reservationId
        );
    }

    public async Task<bool> ProcessApprovalAsync(int approvalId, string approverId, ApprovalStatus status, string? comments = null)
    {
        // Get the approval record with the reservation data
        var approval = await _context.ReservationApprovals
            .Include(a => a.Reservation)
            .FirstOrDefaultAsync(a => a.Id == approvalId && a.ApproverId == approverId);
        
        if (approval == null || approval.Reservation == null)
        {
            return false;
        }
        
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Update the approval
            approval.Status = status;
            approval.Comments = comments;
            approval.ApprovalDate = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            // Handle rejection - simple case
            if (status == ApprovalStatus.Rejected)
            {
                approval.Reservation.Status = ReservationStatus.Rejected;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                await LogApprovalAction(approverId, approval, status);
                return true;
            }
            
            // Handle approval
            if (status == ApprovalStatus.Approved)
            {
                // Check previous approvals
                bool canApprove = await CanApproveAtThisLevel(approval);
                if (!canApprove)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                
                // Update reservation status based on approval level
                bool isFinalApproval = await IsFinalApprovalLevel(approval);
                approval.Reservation.Status = isFinalApproval ? 
                    ReservationStatus.Approved : 
                    ReservationStatus.PartiallyApproved;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                await LogApprovalAction(approverId, approval, status);
                return true;
            }
            
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<bool> CanApproveAtThisLevel(ReservationApproval approval)
    {
        // First check if the reservation is already rejected
        if (approval.Reservation.Status == ReservationStatus.Rejected)
        {
            return false;
        }

        // Check if any approvals at previous levels have been rejected
        var anyRejections = await _context.ReservationApprovals
            .Where(a => a.ReservationId == approval.ReservationId && 
                   a.ApprovalLevel < approval.ApprovalLevel && 
                   a.Status == ApprovalStatus.Rejected)
            .AnyAsync();
            
        if (anyRejections)
        {
            return false;
        }
        
        // Check if all approvals at previous levels are approved
        for (int i = 1; i < approval.ApprovalLevel; i++)
        {
            var previousApproval = await _context.ReservationApprovals
                .Where(a => a.ReservationId == approval.ReservationId && a.ApprovalLevel == i)
                .FirstOrDefaultAsync();
            
            if (previousApproval == null || previousApproval.Status != ApprovalStatus.Approved)
            {
                return false;
            }
        }
        
        return true;
    }

    private async Task<bool> IsFinalApprovalLevel(ReservationApproval approval)
    {
        int maxLevel = await _context.ReservationApprovals
            .Where(a => a.ReservationId == approval.ReservationId)
            .MaxAsync(a => a.ApprovalLevel);
        
        return approval.ApprovalLevel == maxLevel;
    }

    private async Task LogApprovalAction(string approverId, ReservationApproval approval, ApprovalStatus status)
    {
        var approver = await _context.Users.FindAsync(approverId);
        
        await _logService.LogActivityAsync(
            approverId,
            approver?.UserName ?? "System",
            status == ApprovalStatus.Approved ? "Approve" : "Reject",
            "ReservationApproval",
            $"{(status == ApprovalStatus.Approved ? "Approved" : "Rejected")} reservation ID {approval.Reservation?.Id} at level {approval.ApprovalLevel}",
            approval.Reservation?.Id
        );
    }

    public async Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime startDate, DateTime endDate, int? excludeReservationId = null)
    {
        var query = _context.VehicleReservations
            .Where(r => r.VehicleId == vehicleId &&
                      r.Status != ReservationStatus.Cancelled &&
                      r.Status != ReservationStatus.Rejected);
                      
        // Check for date overlapping
        query = query.Where(r => 
            (r.StartDate <= startDate && r.EndDate >= startDate) ||
            (r.StartDate <= endDate && r.EndDate >= endDate) ||
            (r.StartDate >= startDate && r.EndDate <= endDate));
        
        // Exclude specific reservation if needed
        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<bool> CheckDriverAvailabilityAsync(int driverId, DateTime startDate, DateTime endDate, int? excludeReservationId = null)
    {
        var query = _context.VehicleReservations
            .Where(r => r.DriverId == driverId &&
                       r.Status != ReservationStatus.Cancelled &&
                       r.Status != ReservationStatus.Rejected);
                       
        // Check for date overlapping
        query = query.Where(r => 
            (r.StartDate <= startDate && r.EndDate >= startDate) ||
            (r.StartDate <= endDate && r.EndDate >= endDate) ||
            (r.StartDate >= startDate && r.EndDate <= endDate));
        
        // Exclude specific reservation if needed
        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<Dictionary<DateTime, int>> GetReservationStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var result = CreateEmptyDateRangeDictionary(startDate, endDate);
        
        // Get the count of active reservations for each day
        var activeReservations = await GetActiveReservationsInDateRange(startDate, endDate);
        
        // Count active reservations for each day
        foreach (var reservation in activeReservations)
        {
            CountReservationDays(reservation, startDate, endDate, result);
        }
        
        return result;
    }
    
    private Dictionary<DateTime, int> CreateEmptyDateRangeDictionary(DateTime startDate, DateTime endDate)
    {
        var result = new Dictionary<DateTime, int>();
        
        // Create empty entries for all days in the range
        for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            result[date] = 0;
        }
        
        return result;
    }
    
    private async Task<List<VehicleReservation>> GetActiveReservationsInDateRange(DateTime startDate, DateTime endDate)
    {
        return await _context.VehicleReservations
            .Where(r => r.Status != ReservationStatus.Cancelled &&
                        r.Status != ReservationStatus.Rejected &&
                        ((r.StartDate <= startDate && r.EndDate >= startDate) ||
                         (r.StartDate <= endDate && r.EndDate >= endDate) ||
                         (r.StartDate >= startDate && r.EndDate <= endDate)))
            .ToListAsync();
    }
    
    private void CountReservationDays(VehicleReservation reservation, DateTime startDate, DateTime endDate, Dictionary<DateTime, int> result)
    {
        DateTime countStart = reservation.StartDate > startDate ? reservation.StartDate : startDate;
        DateTime countEnd = reservation.EndDate < endDate ? reservation.EndDate : endDate;
        
        for (DateTime date = countStart.Date; date <= countEnd.Date; date = date.AddDays(1))
        {
            if (result.ContainsKey(date))
            {
                result[date]++;
            }
        }
    }

    public async Task<byte[]> ExportReservationsToExcelAsync(DateTime startDate, DateTime endDate, ReservationStatus? status = null)
    {
        var reservations = await _context.VehicleReservations
            .Include(r => r.Requester)
            .Include(r => r.Vehicle)
            .Include(r => r.Driver)
            .Where(r => (r.StartDate >= startDate && r.StartDate <= endDate) ||
                        (r.EndDate >= startDate && r.EndDate <= endDate) ||
                        (r.StartDate <= startDate && r.EndDate >= endDate))
            .OrderBy(r => r.StartDate)
            .ToListAsync();
        
        if (status.HasValue)
        {
            reservations = reservations.Where(r => r.Status == status.Value).ToList();
        }
        
        // Implementasi menggunakan ClosedXML
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Vehicle Reservations");
        
        // Add header row
        worksheet.Cell(1, 1).Value = "Reservation ID";
        worksheet.Cell(1, 2).Value = "Purpose";
        worksheet.Cell(1, 3).Value = "Requester";
        worksheet.Cell(1, 4).Value = "Start Date";
        worksheet.Cell(1, 5).Value = "End Date";
        worksheet.Cell(1, 6).Value = "Vehicle";
        worksheet.Cell(1, 7).Value = "Driver";
        worksheet.Cell(1, 8).Value = "Origin";
        worksheet.Cell(1, 9).Value = "Destination";
        worksheet.Cell(1, 10).Value = "Status";
        worksheet.Cell(1, 11).Value = "Request Date";
        
        // Style header row
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRow.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        
        // Add data rows
        int row = 2;
        foreach (var reservation in reservations)
        {
            worksheet.Cell(row, 1).Value = reservation.Id;
            worksheet.Cell(row, 2).Value = reservation.Purpose;
            worksheet.Cell(row, 3).Value = reservation.Requester?.FullName ?? reservation.Requester?.UserName ?? "Unknown";
            worksheet.Cell(row, 4).Value = reservation.StartDate;
            worksheet.Cell(row, 4).Style.DateFormat.Format = "yyyy-MM-dd";
            worksheet.Cell(row, 5).Value = reservation.EndDate;
            worksheet.Cell(row, 5).Style.DateFormat.Format = "yyyy-MM-dd";
            worksheet.Cell(row, 6).Value = $"{reservation.Vehicle?.Brand} {reservation.Vehicle?.Model} ({reservation.Vehicle?.RegistrationNumber})";
            worksheet.Cell(row, 7).Value = reservation.Driver?.Name ?? "Not Assigned";
            worksheet.Cell(row, 8).Value = reservation.Origin;
            worksheet.Cell(row, 9).Value = reservation.Destination;
            worksheet.Cell(row, 10).Value = reservation.Status.ToString();
            worksheet.Cell(row, 11).Value = reservation.RequestDate;
            worksheet.Cell(row, 11).Style.DateFormat.Format = "yyyy-MM-dd";
            
            row++;
        }
        
        // Auto fit columns
        worksheet.Columns().AdjustToContents();
        
        // Return as byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
