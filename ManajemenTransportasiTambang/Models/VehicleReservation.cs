using System.ComponentModel.DataAnnotations;

namespace ManajemenTransportasiTambang.Models;

public class VehicleReservation
{
    public int Id { get; set; }
    
    [Required]
    public string Purpose { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public string? Origin { get; set; }
    
    public string? Destination { get; set; }
    
    public int EstimatedDistance { get; set; } // in kilometers
    
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    
    public string? CancellationReason { get; set; }
    
    public DateTime RequestDate { get; set; } = DateTime.Now;
    
    public string? Notes { get; set; }
    
    // Foreign keys
    public string RequesterId { get; set; } = string.Empty;
    public ApplicationUser? Requester { get; set; }
    
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    
    public int DriverId { get; set; }
    public Driver? Driver { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime LastModified { get; set; } = DateTime.Now;
    
    public string? CreatedBy { get; set; }
    
    public string? ModifiedBy { get; set; }
    
    // Navigation properties
    public ICollection<ReservationApproval>? Approvals { get; set; }
    public ICollection<ActivityLog>? ActivityLogs { get; set; }
}

public enum ReservationStatus
{
    Pending,
    PartiallyApproved,
    Approved,
    Rejected,
    Completed,
    Cancelled
}
