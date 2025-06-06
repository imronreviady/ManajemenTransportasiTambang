namespace ManajemenTransportasiTambang.Models;

public class Region
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime LastModified { get; set; } = DateTime.Now;
    
    public string? CreatedBy { get; set; }
    
    public string? ModifiedBy { get; set; }
    
    // Navigation properties
    public ICollection<Location>? Locations { get; set; }
}
