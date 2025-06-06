using Microsoft.AspNetCore.Identity;

namespace ManajemenTransportasiTambang.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    
    // Approval level (for hierarchical approval)
    public int ApprovalLevel { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime LastModified { get; set; } = DateTime.Now;
    
    public string? CreatedBy { get; set; }
    
    public string? ModifiedBy { get; set; }
    
    // Navigation properties
    public ICollection<VehicleReservation>? RequestedReservations { get; set; }
    public ICollection<ReservationApproval>? GivenApprovals { get; set; }
}
