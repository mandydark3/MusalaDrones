using System.ComponentModel.DataAnnotations;

namespace MusalaDrones.Model
{
    public enum EDroneModel 
    { 
        [Display(Name = "Lightweight")]
        Lightweight = 1,
        [Display(Name = "Middleweight")]
        Middleweight,
        [Display(Name = "Cruiserweight")]
        Cruiserweight,
        [Display(Name = "Heavyweight")]
        Heavyweight 
    }

    public enum EDroneState { IDLE = 1, LOADING, LOADED, DELIVERING, DELIVERED, RETURNING }

    public class Drone
    {
        public Guid ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        public EDroneModel Model { get; set; }

        [Required]
        [Range(0, 500)]
        public float WeightLimit { get; set; }

        [Required]
        [Range(0, 100)]
        public int BatteryCapacity { get; set; }

        [Required]
        public EDroneState State { get; set; }

        public virtual List<DroneMedication> DroneMedications { get; set; } = new List<DroneMedication>();
    }
}
