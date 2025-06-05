using System.ComponentModel.DataAnnotations;

namespace ManajemenTransportasiTambang.Models;

public class Driver
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? LicenseNumber { get; set; }
    
    public DateTime LicenseExpiry { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? Address { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    // Navigation properties
    public ICollection<VehicleReservation>? Assignments { get; set; }
}
