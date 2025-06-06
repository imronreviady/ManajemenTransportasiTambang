namespace ManajemenTransportasiTambang.Models;

public class MaintenanceRecord
{
    public int Id { get; set; }
    public DateTime ServiceDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string? ServiceProvider { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime LastModified { get; set; } = DateTime.Now;
    
    public string? CreatedBy { get; set; }
    
    public string? ModifiedBy { get; set; }
    
    // Foreign key for Vehicle
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}
