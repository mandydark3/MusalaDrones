namespace MusalaDrones.Model.AuxiliaryModels
{
    public class DroneLoadingInfoAM
    {
        public string SerialNumber { get; set; } = string.Empty;
        public List<Medication> Medications { get; set; } = new List<Medication>();
    }
}
