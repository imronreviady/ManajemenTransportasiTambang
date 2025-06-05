namespace ManajemenTransportasiTambang.Models;

public class Region
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Navigation properties
    public ICollection<Location>? Locations { get; set; }
}
