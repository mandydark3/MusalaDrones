using Microsoft.EntityFrameworkCore;
using MusalaDrones.Model;

namespace MusalaDrones.Data
{
    public class MusalaDronesDbContext : DbContext
    {
        public MusalaDronesDbContext() { }

        public MusalaDronesDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public void Seed()
        {
            Database.EnsureCreated();
            new MusalaDronesSeeder(this).Seed();
        }

        // Entities
        public virtual DbSet<Drone> Drones { get; set; }
        public virtual DbSet<Medication> Medications { get; set; }
        public virtual DbSet<DroneMedication> DronesMedications { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
    }
}
