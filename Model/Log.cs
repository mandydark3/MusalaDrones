using System.ComponentModel.DataAnnotations;

namespace MusalaDrones.Model
{
    public class Log
    {
        public Guid ID { get; set; }

        public Guid DroneID { get; set; }

        public Drone Drone { get; set; } = new Drone();

        [Required]
        public int BatterLevel { get; set; }

        [Required]
        public DateTime Checked { get; set; }
    }
}
