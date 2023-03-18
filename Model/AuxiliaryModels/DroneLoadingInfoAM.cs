namespace MusalaDrones.Model.AuxiliaryModels
{
    public class DroneLoadingInfoAM
    {
        public string SerialNumber { get; set; } = string.Empty;
        // Key - Medication code
        // Value - Quantity
        public List<KeyValuePair<string, int>> Medications{ get; set; } = new List<KeyValuePair<string, int>>();
    }
}
