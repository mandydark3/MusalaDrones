using System.ComponentModel.DataAnnotations;

namespace MusalaDrones.Model
{
    public class Medication
    {
        public Guid ID { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_-].*?$")]
        public string Name { get; set; } = String.Empty;

        [Required]
        public float Weight { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z0-9_]+$")]
        public string Code { get; set; } = string.Empty;

        public byte[]? Image { get; set; }

        public virtual List<DroneMedication> MedicationsDrone { get; set; } = new List<DroneMedication>();
    }
}
