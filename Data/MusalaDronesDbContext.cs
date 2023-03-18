using Microsoft.EntityFrameworkCore;
using MusalaDrones.Model;

namespace MusalaDrones.Data
{
    public class MusalaDronesDbContext : DbContext
    {
        public MusalaDronesDbContext(DbContextOptions options) : base(options)
        {
        }

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
        public DbSet<Drone> Drones { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}
