using Microsoft.EntityFrameworkCore;
using LogiTrack.WebApi.Models;

namespace LogiTrack.WebApi.Data
{
    public class LogiTrackDbContext : DbContext
    {
        public LogiTrackDbContext(DbContextOptions<LogiTrackDbContext> options)
            : base(options) { }

        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Driver> Drivers => Set<Driver>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Shipment>()
                .HasIndex(s => s.Reference);
            base.OnModelCreating(modelBuilder);
        }
    }
}
