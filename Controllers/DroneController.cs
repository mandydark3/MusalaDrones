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
        [HttpPost(Name = "LoadDroneWithMedication")]
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

        // Registering a drone
        [HttpPost(Name = "RegisteringNewDrone")]
        public JsonResult RegisteringDrone(string newDrone)
        {
            var drone = JsonSerializer.Deserialize<Drone>(newDrone);
            if (drone == null)
                return new JsonResult(JsonResults.JSON_WRONGDRONEINFO);

            // Validate drone data
            // Using reflection to get the values of the model attributes guarantees in the future only modify the model itself to change the validations
            var snMaxLenght = drone.SerialNumber.GetType().GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute), true)[0];
            if (snMaxLenght != null && drone.SerialNumber.Length > snMaxLenght.ToString().Length)
                return new JsonResult(JsonResults.JSON_WRONGDRONEINFO);

            var WeightLimit = drone.WeightLimit.GetType().GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RangeAttribute), true);
            if (WeightLimit != null)
            {
                float minWeight = int.Parse(WeightLimit[0].ToString());
                float maxWeight = int.Parse(WeightLimit[1].ToString());
                drone.WeightLimit = Math.Clamp(drone.WeightLimit, minWeight, maxWeight);
            }

            // Max percent is always 100 and default state is IDLE
            drone.BatteryCapacity = 100;
            drone.State = EDroneState.IDLE;

            _context.Drones.Add(drone);
            _context.SaveChanges();

            return new JsonResult(JsonResults.JSON_OK);
        }
    }
}
