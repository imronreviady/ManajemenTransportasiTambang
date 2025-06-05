using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ManajemenTransportasiTambang.Models;

namespace ManajemenTransportasiTambang.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Region> Regions { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<VehicleReservation> VehicleReservations { get; set; }
    public DbSet<ReservationApproval> ReservationApprovals { get; set; }
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
    public DbSet<FuelConsumptionRecord> FuelConsumptionRecords { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships
        builder.Entity<Location>()
            .HasOne(l => l.Region)
            .WithMany(r => r.Locations)
            .HasForeignKey(l => l.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Vehicle>()
            .HasOne(v => v.Location)
            .WithMany(l => l.AssignedVehicles)
            .HasForeignKey(v => v.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VehicleReservation>()
            .HasOne(vr => vr.Requester)
            .WithMany(u => u.RequestedReservations)
            .HasForeignKey(vr => vr.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VehicleReservation>()
            .HasOne(vr => vr.Vehicle)
            .WithMany(v => v.Reservations)
            .HasForeignKey(vr => vr.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VehicleReservation>()
            .HasOne(vr => vr.Driver)
            .WithMany(d => d.Assignments)
            .HasForeignKey(vr => vr.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ReservationApproval>()
            .HasOne(ra => ra.Reservation)
            .WithMany(vr => vr.Approvals)
            .HasForeignKey(ra => ra.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ReservationApproval>()
            .HasOne(ra => ra.Approver)
            .WithMany(u => u.GivenApprovals)
            .HasForeignKey(ra => ra.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MaintenanceRecord>()
            .HasOne(mr => mr.Vehicle)
            .WithMany(v => v.MaintenanceRecords)
            .HasForeignKey(mr => mr.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FuelConsumptionRecord>()
            .HasOne(fcr => fcr.Vehicle)
            .WithMany(v => v.FuelConsumptionRecords)
            .HasForeignKey(fcr => fcr.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ActivityLog>()
            .HasOne(al => al.Reservation)
            .WithMany(vr => vr.ActivityLogs)
            .HasForeignKey(al => al.ReservationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
