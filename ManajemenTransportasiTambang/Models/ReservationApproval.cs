using System.ComponentModel.DataAnnotations;

namespace ManajemenTransportasiTambang.Models;

public class ReservationApproval
{
    public int Id { get; set; }
    
    public int ApprovalLevel { get; set; }
    
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    
    public DateTime? ApprovalDate { get; set; }
    
    public string? Comments { get; set; }
    
    // Foreign keys
    public int ReservationId { get; set; }
    public VehicleReservation? Reservation { get; set; }
    
    public string ApproverId { get; set; } = string.Empty;
    public ApplicationUser? Approver { get; set; }
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}
