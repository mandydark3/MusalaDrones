namespace MusalaDrones.Model
{
    public class DroneMedication
    {
        public Guid ID { get; set; }
        public Guid DroneID { get; set; }
        public Drone Drone { get; set; } = new Drone();
        public Guid MedicationID { get; set; }
        public Medication Medication { get; set; } = new Medication();
        public int Quantity { get; set; }
    }
}
