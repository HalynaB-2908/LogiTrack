using LogiTrack.WebApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.WebApi.Data
{
    public class LogiTrackDbContext : IdentityDbContext<ApplicationUser>
    {
        public LogiTrackDbContext(DbContextOptions<LogiTrackDbContext> options)
            : base(options) { }

        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Driver> Drivers => Set<Driver>();

        public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Shipment>()
                .HasIndex(s => s.Reference);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Shipments)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Vehicle)
                .WithMany(v => v.Shipments)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Driver)
                .WithMany(d => d.Vehicles)
                .HasForeignKey(v => v.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ApiKey>()
                .HasIndex(k => k.Key)
                .IsUnique();
        }
    }
}
