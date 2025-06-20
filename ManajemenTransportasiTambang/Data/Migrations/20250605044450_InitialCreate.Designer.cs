﻿// <auto-generated />
using System;
using ManajemenTransportasiTambang.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ManajemenTransportasiTambang.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250605044450_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.ActivityLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Details")
                        .HasColumnType("longtext");

                    b.Property<string>("IpAddress")
                        .HasColumnType("longtext");

                    b.Property<string>("Module")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("ReservationId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("ReservationId");

                    b.ToTable("ActivityLogs");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<int>("ApprovalLevel")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Department")
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("FullName")
                        .HasColumnType("longtext");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Position")
                        .HasColumnType("longtext");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.Driver", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Address")
                        .HasColumnType("longtext");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("LicenseExpiry")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("LicenseNumber")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Drivers");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.FuelConsumptionRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<decimal>("Cost")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("FillDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Notes")
                        .HasColumnType("longtext");

                    b.Property<int>("Odometer")
                        .HasColumnType("int");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("ReservationId")
                        .HasColumnType("int");

                    b.Property<int>("VehicleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ReservationId");

                    b.HasIndex("VehicleId");

                    b.ToTable("FuelConsumptionRecords");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.Location", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Address")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("RegionId")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RegionId");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.MaintenanceRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<decimal>("Cost")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Notes")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("ServiceDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ServiceProvider")
                        .HasColumnType("longtext");

                    b.Property<int>("VehicleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("VehicleId");

                    b.ToTable("MaintenanceRecords");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.Region", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Regions");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.ReservationApproval", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("ApprovalDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("ApprovalLevel")
                        .HasColumnType("int");

                    b.Property<string>("ApproverId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Comments")
                        .HasColumnType("longtext");

                    b.Property<int>("ReservationId")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ApproverId");

                    b.HasIndex("ReservationId");

                    b.ToTable("ReservationApprovals");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.Vehicle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Brand")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Capacity")
                        .HasColumnType("int");

                    b.Property<double>("FuelConsumptionRate")
                        .HasColumnType("double");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("LastServiceDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("LocationId")
                        .HasColumnType("int");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("NextServiceDueDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Notes")
                        .HasColumnType("longtext");

                    b.Property<int>("Ownership")
                        .HasColumnType("int");

                    b.Property<string>("RegistrationNumber")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RentalCompany")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("RentalEndDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("RentalStartDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.ToTable("Vehicles");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.VehicleReservation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("CancellationReason")
                        .HasColumnType("longtext");

                    b.Property<string>("Destination")
                        .HasColumnType("longtext");

                    b.Property<int>("DriverId")
                        .HasColumnType("int");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("EstimatedDistance")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasColumnType("longtext");

                    b.Property<string>("Origin")
                        .HasColumnType("longtext");

                    b.Property<string>("Purpose")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("RequestDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("RequesterId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("VehicleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DriverId");

                    b.HasIndex("RequesterId");

                    b.HasIndex("VehicleId");

                    b.ToTable("VehicleReservations");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.ActivityLog", b =>
                {
                    b.HasOne("ManajemenTransportasiTambang.Models.VehicleReservation", "Reservation")
                        .WithMany("ActivityLogs")
                        .HasForeignKey("ReservationId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Reservation");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.FuelConsumptionRecord", b =>
                {
                    b.HasOne("ManajemenTransportasiTambang.Models.VehicleReservation", "Reservation")
                        .WithMany()
                        .HasForeignKey("ReservationId");

                    b.HasOne("ManajemenTransportasiTambang.Models.Vehicle", "Vehicle")
                        .WithMany("FuelConsumptionRecords")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Reservation");

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.Location", b =>
                {
                    b.HasOne("ManajemenTransportasiTambang.Models.Region", "Region")
                        .WithMany("Locations")
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Region");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.MaintenanceRecord", b =>
                {
                    b.HasOne("ManajemenTransportasiTambang.Models.Vehicle", "Vehicle")
                        .WithMany("MaintenanceRecords")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.ReservationApproval", b =>
                {
                    b.HasOne("ManajemenTransportasiTambang.Models.ApplicationUser", "Approver")
                        .WithMany("GivenApprovals")
                        .HasForeignKey("ApproverId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ManajemenTransportasiTambang.Models.VehicleReservation", "Reservation")
                        .WithMany("Approvals")
                        .HasForeignKey("ReservationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Approver");

                    b.Navigation("Reservation");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.Vehicle", b =>
                {
                    b.HasOne("ManajemenTransportasiTambang.Models.Location", "Location")
                        .WithMany("AssignedVehicles")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Location");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.VehicleReservation", b =>
                {
                    b.HasOne("ManajemenTransportasiTambang.Models.Driver", "Driver")
                        .WithMany("Assignments")
                        .HasForeignKey("DriverId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ManajemenTransportasiTambang.Models.ApplicationUser", "Requester")
                        .WithMany("RequestedReservations")
                        .HasForeignKey("RequesterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ManajemenTransportasiTambang.Models.Vehicle", "Vehicle")
                        .WithMany("Reservations")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Driver");

                    b.Navigation("Requester");

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("ManajemenTransportasiTambang.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("ManajemenTransportasiTambang.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ManajemenTransportasiTambang.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("ManajemenTransportasiTambang.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.ApplicationUser", b =>
                {
                    b.Navigation("GivenApprovals");

                    b.Navigation("RequestedReservations");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.Driver", b =>
                {
                    b.Navigation("Assignments");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.Location", b =>
                {
                    b.Navigation("AssignedVehicles");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.Region", b =>
                {
                    b.Navigation("Locations");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.Vehicle", b =>
                {
                    b.Navigation("FuelConsumptionRecords");

                    b.Navigation("MaintenanceRecords");

                    b.Navigation("Reservations");
                });

            modelBuilder.Entity("ManajemenTransportasiTambang.Models.VehicleReservation", b =>
                {
                    b.Navigation("ActivityLogs");

                    b.Navigation("Approvals");
                });
#pragma warning restore 612, 618
        }
    }
}
