using ManajemenTransportasiTambang.Models;

namespace ManajemenTransportasiTambang.Services;

public interface ILogService
{
    Task LogActivityAsync(string userId, string username, string action, string module, string? details = null, int? reservationId = null, string? ipAddress = null);
    Task<IEnumerable<ActivityLog>> GetRecentLogsAsync(int count = 100);
    Task<IEnumerable<ActivityLog>> GetLogsByUserAsync(string userId, int count = 100);
    Task<IEnumerable<ActivityLog>> GetLogsByModuleAsync(string module, int count = 100);
    Task<IEnumerable<ActivityLog>> GetLogsByReservationAsync(int reservationId);
}
