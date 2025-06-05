using Microsoft.AspNetCore.Identity;

namespace ManajemenTransportasiTambang.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    
    // Approval level (for hierarchical approval)
    public int ApprovalLevel { get; set; }
    
    // Navigation properties
    public ICollection<VehicleReservation>? RequestedReservations { get; set; }
    public ICollection<ReservationApproval>? GivenApprovals { get; set; }
}
