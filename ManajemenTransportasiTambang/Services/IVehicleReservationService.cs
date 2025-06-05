using ManajemenTransportasiTambang.Models;

namespace ManajemenTransportasiTambang.Services;

public interface IVehicleReservationService
{
    Task<IEnumerable<VehicleReservation>> GetAllReservationsAsync();
    Task<VehicleReservation?> GetReservationByIdAsync(int id);
    Task<IEnumerable<VehicleReservation>> GetReservationsByRequesterAsync(string requesterId);
    Task<IEnumerable<VehicleReservation>> GetReservationsByStatusAsync(ReservationStatus status);
    Task<IEnumerable<VehicleReservation>> GetReservationsByVehicleAsync(int vehicleId);
    Task<IEnumerable<VehicleReservation>> GetReservationsByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<ReservationApproval>> GetApprovalsByReservationIdAsync(int reservationId);
    Task<VehicleReservation> CreateReservationAsync(VehicleReservation reservation, List<string> approverIds);
    Task UpdateReservationStatusAsync(int reservationId, ReservationStatus status, string? cancellationReason = null);
    Task<bool> ProcessApprovalAsync(int approvalId, string approverId, ApprovalStatus status, string? comments = null);
    Task<bool> CheckVehicleAvailabilityAsync(int vehicleId, DateTime startDate, DateTime endDate, int? excludeReservationId = null);
    Task<bool> CheckDriverAvailabilityAsync(int driverId, DateTime startDate, DateTime endDate, int? excludeReservationId = null);
    Task<Dictionary<DateTime, int>> GetReservationStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<byte[]> ExportReservationsToExcelAsync(DateTime startDate, DateTime endDate, ReservationStatus? status = null);
}
