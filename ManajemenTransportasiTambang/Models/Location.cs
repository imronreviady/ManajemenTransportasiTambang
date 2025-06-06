namespace ManajemenTransportasiTambang.Models;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public LocationType Type { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime LastModified { get; set; } = DateTime.Now;
    
    public string? CreatedBy { get; set; }
    
    public string? ModifiedBy { get; set; }
    
    // Foreign key for Region
    public int RegionId { get; set; }
    public Region? Region { get; set; }
    
    // Navigation properties
    public ICollection<Vehicle>? AssignedVehicles { get; set; }
}

public enum LocationType
{
    HeadOffice,
    BranchOffice,
    Mine
}
