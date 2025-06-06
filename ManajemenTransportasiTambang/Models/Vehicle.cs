using System.ComponentModel.DataAnnotations;

namespace ManajemenTransportasiTambang.Models;

public class Vehicle
{
    public int Id { get; set; }
    
    [Required]
    public string RegistrationNumber { get; set; } = string.Empty;
    
    [Required]
    public string Brand { get; set; } = string.Empty;
    
    [Required]
    public string Model { get; set; } = string.Empty;
    
    public int Year { get; set; }
    
    [Required]
    public VehicleType Type { get; set; }
    
    [Required]
    public VehicleOwnership Ownership { get; set; }
    
    public string? RentalCompany { get; set; }
    
    public DateTime? RentalStartDate { get; set; }
    
    public DateTime? RentalEndDate { get; set; }
    
    public int Capacity { get; set; }

    public double FuelConsumptionRate { get; set; } // L/100km
    
    public DateTime LastServiceDate { get; set; }

    public DateTime NextServiceDueDate { get; set; }
    
    public string? Notes { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Foreign key for Location
    public int LocationId { get; set; }
    public Location? Location { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime LastModified { get; set; } = DateTime.Now;
    
    public string? CreatedBy { get; set; }
    
    public string? ModifiedBy { get; set; }
    
    // Navigation properties
    public ICollection<VehicleReservation>? Reservations { get; set; }
    public ICollection<MaintenanceRecord>? MaintenanceRecords { get; set; }
    public ICollection<FuelConsumptionRecord>? FuelConsumptionRecords { get; set; }
}

public enum VehicleType
{
    PassengerCar,
    Bus,
    Truck,
    PickupTruck,
    HeavyEquipment
}

public enum VehicleOwnership
{
    OwnedByCompany,
    Rented
}

