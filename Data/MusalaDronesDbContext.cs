using Microsoft.EntityFrameworkCore;
using MusalaDrones.Model;

namespace MusalaDrones.Data
{
    public class MusalaDronesDbContext : DbContext
    {
        public MusalaDronesDbContext(DbContextOptions options) : base(options)
        {

        }

        // Entities
        public DbSet<Drone> Drones { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}
