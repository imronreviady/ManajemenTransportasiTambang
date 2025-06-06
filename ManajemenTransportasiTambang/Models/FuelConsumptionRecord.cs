namespace ManajemenTransportasiTambang.Models;

public class FuelConsumptionRecord
{
    public int Id { get; set; }
    public DateTime FillDate { get; set; }
    public decimal Quantity { get; set; } // in liters
    public decimal Cost { get; set; }
    public int Odometer { get; set; } // in kilometers
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime LastModified { get; set; } = DateTime.Now;
    
    public string? CreatedBy { get; set; }
    
    public string? ModifiedBy { get; set; }
    
    // Foreign key for Vehicle
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    
    // Optional - link to a specific reservation
    public int? ReservationId { get; set; }
    public VehicleReservation? Reservation { get; set; }
}
