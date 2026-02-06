using LuxRentals.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Data;

public class LuxRentalsDbContext : IdentityDbContext
{
    public LuxRentalsDbContext()
    {
    }

    public LuxRentalsDbContext(DbContextOptions<LuxRentalsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }

    public DbSet<BookingStatus> BookingStatuses { get; set; }

    public DbSet<Car> Cars { get; set; }

    public DbSet<CarStatus> CarStatuses { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<FuelType> FuelTypes { get; set; }

    public DbSet<Make> Makes { get; set; }

    public DbSet<Model> Models { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<VehicleClass> VehicleClasses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.PkBookingId).HasName("PK__Booking__8A399DE0B0FB731A");

            entity.ToTable("Booking");

            entity.Property(e => e.PkBookingId).HasColumnName("pkBookingId");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelledAt");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.EndDateTime).HasColumnName("endDateTime");
            entity.Property(e => e.FkBookingStatusId).HasColumnName("fkBookingStatusId");
            entity.Property(e => e.FkCarId).HasColumnName("fkCarId");
            entity.Property(e => e.FkCustomerId).HasColumnName("fkCustomerId");
            entity.Property(e => e.StartDateTime).HasColumnName("startDateTime");

            entity.HasOne(d => d.FkBookingStatus).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.FkBookingStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_Status");

            entity.HasOne(d => d.FkCar).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.FkCarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_Car");

            entity.HasOne(d => d.FkCustomer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.FkCustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_Customer");
        });

        modelBuilder.Entity<BookingStatus>(entity =>
        {
            entity.HasKey(e => e.PkBookingStatusId).HasName("PK__BookingS__9B31AD4290225E69");

            entity.ToTable("BookingStatus");

            entity.Property(e => e.PkBookingStatusId).HasColumnName("pkBookingStatusId");
            entity.Property(e => e.BookingStatus1)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("bookingStatus");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.PkCarId).HasName("PK__Car__042F2AAC6BD16B16");

            entity.ToTable("Car");

            entity.HasIndex(e => e.LicencePlate, "UQ__Car__1771C83D56CF7E7E").IsUnique();

            entity.HasIndex(e => e.VinNumber, "UQ__Car__96F3ADF410EA4174").IsUnique();

            entity.Property(e => e.PkCarId).HasColumnName("pkCarId");
            entity.Property(e => e.CarThumbnail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("carThumbnail");
            entity.Property(e => e.Colour)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("colour");
            entity.Property(e => e.DailyRate)
                .HasColumnType("decimal(19, 2)")
                .HasColumnName("dailyRate");
            entity.Property(e => e.FkCarStatusId).HasColumnName("fkCarStatusId");
            entity.Property(e => e.FkFuelTypeId).HasColumnName("fkFuelTypeId");
            entity.Property(e => e.FkModelId).HasColumnName("fkModelId");
            entity.Property(e => e.FkVehicleClassId).HasColumnName("fkVehicleClassId");
            entity.Property(e => e.LicencePlate)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("licencePlate");
            entity.Property(e => e.LuggageCap).HasColumnName("luggageCap");
            entity.Property(e => e.PersonCap).HasColumnName("personCap");
            entity.Property(e => e.TransmissionType).HasColumnName("transmissionType");
            entity.Property(e => e.VinNumber)
                .HasMaxLength(17)
                .IsUnicode(false)
                .HasColumnName("vinNumber");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.FkCarStatus).WithMany(p => p.Cars)
                .HasForeignKey(d => d.FkCarStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Car_CarStatus");

            entity.HasOne(d => d.FkFuelType).WithMany(p => p.Cars)
                .HasForeignKey(d => d.FkFuelTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Car_FuelType");

            entity.HasOne(d => d.FkModel).WithMany(p => p.Cars)
                .HasForeignKey(d => d.FkModelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Car_Model");

            entity.HasOne(d => d.FkVehicleClass).WithMany(p => p.Cars)
                .HasForeignKey(d => d.FkVehicleClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Car_VehicleClass");
        });

        modelBuilder.Entity<CarStatus>(entity =>
        {
            entity.HasKey(e => e.PkCarStatusId).HasName("PK__CarStatu__18420CBD73A9C723");

            entity.ToTable("CarStatus");

            entity.Property(e => e.PkCarStatusId).HasColumnName("pkCarStatusId");
            entity.Property(e => e.StatusFlag)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("statusFlag");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.PkCustomerId).HasName("PK__Customer__1FD9D5A2DFB315FE");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.DriverLicenceNo, "UQ__Customer__021FFF575E2F467D").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Customer__AB6E6164F79866D8").IsUnique();

            entity.Property(e => e.PkCustomerId).HasColumnName("pkCustomerId");
            entity.Property(e => e.DriverLicenceNo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("driverLicenceNo");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("firstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("lastName");
            entity.Property(e => e.LicenceVerified).HasColumnName("licenceVerified");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phoneNumber");
        });

        modelBuilder.Entity<FuelType>(entity =>
        {
            entity.HasKey(e => e.PkFuelTypeId).HasName("PK__FuelType__8BECB883C1BFCF4F");

            entity.ToTable("FuelType");

            entity.Property(e => e.PkFuelTypeId).HasColumnName("pkFuelTypeId");
            entity.Property(e => e.FuelType1)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("fuelType");
        });

        modelBuilder.Entity<Make>(entity =>
        {
            entity.HasKey(e => e.PkMakeId).HasName("PK__Make__DCFC4DDA57C669DE");

            entity.ToTable("Make");

            entity.Property(e => e.PkMakeId).HasColumnName("pkMakeId");
            entity.Property(e => e.MakeName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("makeName");
        });

        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.PkModelId).HasName("PK__Model__C59EF50749A793B2");

            entity.ToTable("Model");

            entity.Property(e => e.PkModelId).HasColumnName("pkModelId");
            entity.Property(e => e.FkMakeId).HasColumnName("fkMakeId");
            entity.Property(e => e.ModelName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modelName");

            entity.HasOne(d => d.FkMake).WithMany(p => p.Models)
                .HasForeignKey(d => d.FkMakeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Model_Make");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.PkTransactionId).HasName("PK__Transact__D335440639A81962");

            entity.ToTable("Transaction");

            entity.Property(e => e.PkTransactionId).HasColumnName("pkTransactionId");
            entity.Property(e => e.AmountPaid)
                .HasColumnType("decimal(19, 2)")
                .HasColumnName("amountPaid");
            entity.Property(e => e.FkBookingId).HasColumnName("fkBookingId");
            entity.Property(e => e.PaymentDate).HasColumnName("paymentDate");

            entity.HasOne(d => d.FkBooking).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.FkBookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaction_Booking");
        });

        modelBuilder.Entity<VehicleClass>(entity =>
        {
            entity.HasKey(e => e.PkVehicleClassId).HasName("PK__VehicleC__0B88CBFE1D5AB50C");

            entity.ToTable("VehicleClass");

            entity.Property(e => e.PkVehicleClassId).HasColumnName("pkVehicleClassId");
            entity.Property(e => e.VehicleClass1)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("vehicleClass");
        });
    }
}
