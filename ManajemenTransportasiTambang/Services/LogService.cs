using ManajemenTransportasiTambang.Data;
using ManajemenTransportasiTambang.Models;
using Microsoft.EntityFrameworkCore;

namespace ManajemenTransportasiTambang.Services;

public class LogService : ILogService
{
    private readonly ApplicationDbContext _context;

    public LogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogActivityAsync(string userId, string username, string action, string module, string? details = null, int? reservationId = null, string? ipAddress = null)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            Username = username,
            Action = action,
            Module = module,
            Details = details,
            ReservationId = reservationId,
            IpAddress = ipAddress,
            Timestamp = DateTime.Now
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ActivityLog>> GetRecentLogsAsync(int count = 100)
    {
        return await _context.ActivityLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<ActivityLog>> GetLogsByUserAsync(string userId, int count = 100)
    {
        return await _context.ActivityLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<ActivityLog>> GetLogsByModuleAsync(string module, int count = 100)
    {
        return await _context.ActivityLogs
            .Where(l => l.Module == module)
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<ActivityLog>> GetLogsByReservationAsync(int reservationId)
    {
        return await _context.ActivityLogs
            .Where(l => l.ReservationId == reservationId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
    }
}
