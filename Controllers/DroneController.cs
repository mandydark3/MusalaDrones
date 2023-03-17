using Microsoft.AspNetCore.Mvc;
using MusalaDrones.Data;
using MusalaDrones.Model;
using MusalaDrones.Model.AuxiliaryModels;
using System.Text.Json;

namespace MusalaDrones.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DroneController : ControllerBase
    {
        public const int LOW_BATTERY_THRESHOLD = 25;

        private readonly ILogger<DroneController> _logger;
        private MusalaDronesDbContext _context;

        public DroneController(ILogger<DroneController> logger, MusalaDronesDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        // Checking drone battery level for a given drone
        [HttpGet(Name = "GetDroneBatteryLevel")]
        public JsonResult GetDroneBatteryLevel(string droneSN)
        {
            var drone = _context.Drones.FirstOrDefault(p => p.SerialNumber == droneSN);
            if (drone == null)
                return new JsonResult(JsonResults.JSON_NOTFOUND);

            return new JsonResult(drone.BatteryCapacity);
        }

        // Checking available drones for loading
        [HttpGet(Name = "GetAvailableDronesLoading")]
        public JsonResult GetAvailableDronesForLoading()
        {
            var result = JsonSerializer.Serialize(_context.Drones.Where(p => p.State == EDroneState.IDLE && p.BatteryCapacity > 25).ToList());
            return new JsonResult(result);
        }

        // Checking loaded medication items for a given drone
        [HttpGet(Name = "GetMedicationFromDrone")]
        public JsonResult GetMedicationFromDrone(string droneSN)
        {
            var drone = _context.Drones.FirstOrDefault(p => p.SerialNumber == droneSN);
            if (drone == null)
                return new JsonResult(JsonResults.JSON_NOTFOUND);

            var result = JsonSerializer.Serialize(drone.Medications.ToList());
            return new JsonResult(result);
        }

        // Loading a drone with medication items
        [HttpGet(Name = "LoadDroneWithMedication")]
        public JsonResult LoadDroneWithMedication(string loadingInfo)
        {
            // Every Input/Output MUST be Json format, therefore we need an Auxiliary Model data type for this
            var linfo = JsonSerializer.Deserialize<DroneLoadingInfoAM>(loadingInfo);
            if (linfo == null)
                return new JsonResult(JsonResults.JSON_WRONGLOADINGINFO);

            var drone = _context.Drones.FirstOrDefault(p => p.SerialNumber == linfo.SerialNumber);
            if (drone == null)
                return new JsonResult(JsonResults.JSON_NOTFOUND);

            // Low battery?
            if (drone.BatteryCapacity <= LOW_BATTERY_THRESHOLD)
                return new JsonResult(JsonResults.JSON_LOWBATTERY);

            // Idle?
            if (drone.State != EDroneState.IDLE)
                return new JsonResult(JsonResults.JSON_NOTAVAILABLE);

            // No medications
            if (linfo.Medications == null || linfo.Medications.Count == 0)
                return new JsonResult(JsonResults.JSON_NOMEDICATIONS);

            List<Medication> medicationLeft = new List<Medication>();
            /* Trying to load the drone with the medications. Two scenarios may occur:
             *  [a] Drone could be loaded and no medication left
             *  [b] Drone could be loaded and some medication left
             */

            // Drone loading
            drone.State = EDroneState.LOADING;
            _context.SaveChanges();

            foreach (var medication in linfo.Medications)
            {
                // Safe null verification
                if (medication == null)
                    continue;

                // Current medication CAN be added
                if (drone.WeightLimit <= drone.Medications.Sum(p => p.Weight) + medication.Weight)
                    drone.Medications.Add(medication);
                else
                    medicationLeft.Add(medication);
            }

            // Drone loaded
            drone.State = EDroneState.LOADED;
            _context.SaveChanges();

            var result = JsonSerializer.Serialize(medicationLeft);
            return new JsonResult(result);
        }
    }
}
