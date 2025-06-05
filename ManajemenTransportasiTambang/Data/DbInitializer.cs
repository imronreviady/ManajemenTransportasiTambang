using ManajemenTransportasiTambang.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ManajemenTransportasiTambang.Data;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            
            // Apply migrations if they're not applied
            context.Database.Migrate();
            
            // Create roles if they don't exist
            string[] roleNames = { "Admin", "Approver" };
            
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            
            // Create admin user if it doesn't exist
            var adminUser = await userManager.FindByNameAsync("admin");
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@mining.com",
                    FullName = "System Administrator",
                    Department = "IT",
                    Position = "System Administrator",
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            
            // Create approvers if they don't exist
            var approver1 = await userManager.FindByNameAsync("approver1");
            
            if (approver1 == null)
            {
                approver1 = new ApplicationUser
                {
                    UserName = "approver1",
                    Email = "approver1@mining.com",
                    FullName = "First Level Approver",
                    Department = "Operations",
                    Position = "Operations Manager",
                    ApprovalLevel = 1,
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(approver1, "Approve123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(approver1, "Approver");
                }
            }
            
            var approver2 = await userManager.FindByNameAsync("approver2");
            
            if (approver2 == null)
            {
                approver2 = new ApplicationUser
                {
                    UserName = "approver2",
                    Email = "approver2@mining.com",
                    FullName = "Second Level Approver",
                    Department = "Fleet Management",
                    Position = "Fleet Director",
                    ApprovalLevel = 2,
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(approver2, "Approve123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(approver2, "Approver");
                }
            }
            
            // Seed regions if empty
            if (!context.Regions.Any())
            {
                var regions = new List<Region>
                {
                    new Region { Name = "North Region", Description = "Northern mining operations" },
                    new Region { Name = "South Region", Description = "Southern mining operations" },
                    new Region { Name = "Central Region", Description = "Central administration" }
                };
                
                context.Regions.AddRange(regions);
                await context.SaveChangesAsync();
            }
            
            // Seed locations if empty
            if (!context.Locations.Any())
            {
                var regions = await context.Regions.ToListAsync();
                
                var locations = new List<Location>
                {
                    new Location 
                    { 
                        Name = "Head Office", 
                        Address = "123 Main Street, Jakarta", 
                        Type = LocationType.HeadOffice, 
                        RegionId = regions.First(r => r.Name == "Central Region").Id
                    },
                    new Location 
                    { 
                        Name = "Branch Office", 
                        Address = "456 Mine Road, Sulawesi", 
                        Type = LocationType.BranchOffice, 
                        RegionId = regions.First(r => r.Name == "North Region").Id
                    },
                    new Location 
                    { 
                        Name = "Mine Site 1", 
                        Address = "Mine Site 1, Sulawesi", 
                        Type = LocationType.Mine, 
                        RegionId = regions.First(r => r.Name == "North Region").Id
                    },
                    new Location 
                    { 
                        Name = "Mine Site 2", 
                        Address = "Mine Site 2, Sulawesi", 
                        Type = LocationType.Mine, 
                        RegionId = regions.First(r => r.Name == "North Region").Id
                    },
                    new Location 
                    { 
                        Name = "Mine Site 3", 
                        Address = "Mine Site 3, Maluku", 
                        Type = LocationType.Mine, 
                        RegionId = regions.First(r => r.Name == "South Region").Id
                    },
                    new Location 
                    { 
                        Name = "Mine Site 4", 
                        Address = "Mine Site 4, Maluku", 
                        Type = LocationType.Mine, 
                        RegionId = regions.First(r => r.Name == "South Region").Id
                    },
                    new Location 
                    { 
                        Name = "Mine Site 5", 
                        Address = "Mine Site 5, Papua", 
                        Type = LocationType.Mine, 
                        RegionId = regions.First(r => r.Name == "South Region").Id
                    },
                    new Location 
                    { 
                        Name = "Mine Site 6", 
                        Address = "Mine Site 6, Papua", 
                        Type = LocationType.Mine, 
                        RegionId = regions.First(r => r.Name == "South Region").Id
                    }
                };
                
                context.Locations.AddRange(locations);
                await context.SaveChangesAsync();
            }

            // Seed vehicles if empty
            if (!context.Vehicles.Any())
            {
                var locations = await context.Locations.ToListAsync();
                
                var vehicles = new List<Vehicle>
                {
                    // Passenger cars
                    new Vehicle
                    {
                        RegistrationNumber = "B 1234 ABC",
                        Brand = "Toyota",
                        Model = "Innova",
                        Year = 2022,
                        Type = VehicleType.PassengerCar,
                        Ownership = VehicleOwnership.OwnedByCompany,
                        Capacity = 7,
                        FuelConsumptionRate = 10.5,
                        LastServiceDate = DateTime.Now.AddMonths(-1),
                        NextServiceDueDate = DateTime.Now.AddMonths(2),
                        IsActive = true,
                        LocationId = locations.First(l => l.Type == LocationType.HeadOffice).Id
                    },
                    new Vehicle
                    {
                        RegistrationNumber = "B 5678 XYZ",
                        Brand = "Toyota",
                        Model = "Fortuner",
                        Year = 2023,
                        Type = VehicleType.PassengerCar,
                        Ownership = VehicleOwnership.OwnedByCompany,
                        Capacity = 7,
                        FuelConsumptionRate = 12.7,
                        LastServiceDate = DateTime.Now.AddMonths(-2),
                        NextServiceDueDate = DateTime.Now.AddMonths(1),
                        IsActive = true,
                        LocationId = locations.First(l => l.Type == LocationType.BranchOffice).Id
                    },
                    
                    // Trucks
                    new Vehicle
                    {
                        RegistrationNumber = "B 9012 DEF",
                        Brand = "Mitsubishi",
                        Model = "Fuso",
                        Year = 2021,
                        Type = VehicleType.Truck,
                        Ownership = VehicleOwnership.OwnedByCompany,
                        Capacity = 8000, // kg
                        FuelConsumptionRate = 18.3,
                        LastServiceDate = DateTime.Now.AddDays(-15),
                        NextServiceDueDate = DateTime.Now.AddMonths(3),
                        IsActive = true,
                        LocationId = locations.First(l => l.Name == "Mine Site 1").Id
                    },
                    
                    // Rental vehicles
                    new Vehicle
                    {
                        RegistrationNumber = "B 3456 GHI",
                        Brand = "Hino",
                        Model = "Dutro",
                        Year = 2022,
                        Type = VehicleType.Truck,
                        Ownership = VehicleOwnership.Rented,
                        RentalCompany = "PT. Sewa Truk Indonesia",
                        RentalStartDate = DateTime.Now.AddMonths(-3),
                        RentalEndDate = DateTime.Now.AddMonths(9),
                        Capacity = 5000, // kg
                        FuelConsumptionRate = 16.8,
                        LastServiceDate = DateTime.Now.AddMonths(-1),
                        NextServiceDueDate = DateTime.Now.AddMonths(2),
                        IsActive = true,
                        LocationId = locations.First(l => l.Name == "Mine Site 2").Id
                    },
                    
                    // Heavy equipment
                    new Vehicle
                    {
                        RegistrationNumber = "B 7890 JKL",
                        Brand = "Caterpillar",
                        Model = "Excavator 320",
                        Year = 2020,
                        Type = VehicleType.HeavyEquipment,
                        Ownership = VehicleOwnership.OwnedByCompany,
                        FuelConsumptionRate = 25.0,
                        LastServiceDate = DateTime.Now.AddMonths(-2),
                        NextServiceDueDate = DateTime.Now.AddMonths(1),
                        IsActive = true,
                        LocationId = locations.First(l => l.Name == "Mine Site 3").Id
                    }
                };
                
                context.Vehicles.AddRange(vehicles);
                await context.SaveChangesAsync();
            }
            
            // Seed drivers if empty
            if (!context.Drivers.Any())
            {
                var drivers = new List<Driver>
                {
                    new Driver
                    {
                        Name = "Budi Santoso",
                        LicenseNumber = "9876543210",
                        LicenseExpiry = DateTime.Now.AddYears(2),
                        PhoneNumber = "081234567890",
                        Address = "Jalan Pahlawan No. 123, Jakarta",
                        IsAvailable = true
                    },
                    new Driver
                    {
                        Name = "Ahmad Dahlan",
                        LicenseNumber = "8765432109",
                        LicenseExpiry = DateTime.Now.AddYears(1),
                        PhoneNumber = "081234567891",
                        Address = "Jalan Merdeka No. 45, Jakarta",
                        IsAvailable = true
                    },
                    new Driver
                    {
                        Name = "Wayan Suparta",
                        LicenseNumber = "7654321098",
                        LicenseExpiry = DateTime.Now.AddMonths(18),
                        PhoneNumber = "081234567892",
                        Address = "Jalan Diponegoro No. 67, Makassar",
                        IsAvailable = true
                    }
                };
                
                context.Drivers.AddRange(drivers);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
