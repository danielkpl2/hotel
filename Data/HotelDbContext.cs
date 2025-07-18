using Microsoft.EntityFrameworkCore;
using Hotel.Models;

namespace Hotel.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    public DbSet<Models.Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Hotel configuration
        modelBuilder.Entity<Models.Hotel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
        });

        // RoomType configuration
        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MaxOccupancy).IsRequired();

            // Add unique constraint on Name
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Room configuration
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoomNumber).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

            // Add unique constraint on RoomNumber and HotelId combination
            entity.HasIndex(e => new { e.RoomNumber, e.HotelId }).IsUnique();

            // Configure Room -> Hotel relationship (many-to-one)
            entity.HasOne(r => r.Hotel)
                  .WithMany(h => h.Rooms)
                  .HasForeignKey(r => r.HotelId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure Room -> RoomType relationship (many-to-one)
            entity.HasOne(r => r.RoomType)
                  .WithMany(rt => rt.Rooms)
                  .HasForeignKey(r => r.RoomTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Booking configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GuestName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BookingReference).HasMaxLength(50);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PeopleCount).IsRequired();

            // Add unique constraint on BookingReference
            entity.HasIndex(e => e.BookingReference).IsUnique();

            // Configure Booking -> Hotel relationship (many-to-one)
            entity.HasOne(b => b.Hotel)
                  .WithMany()
                  .HasForeignKey(b => b.HotelId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Configure Booking <-> Room many-to-many relationship
            entity.HasMany(b => b.Rooms)
                  .WithMany(r => r.Bookings)
                  .UsingEntity<Dictionary<string, object>>(
                      "BookingRooms",
                      j => j.HasOne<Room>().WithMany().HasForeignKey("RoomId"),
                      j => j.HasOne<Booking>().WithMany().HasForeignKey("BookingId"),
                      j =>
                      {
                          j.HasKey("BookingId", "RoomId");
                          j.ToTable("BookingRooms");
                      });
        });
    }
}