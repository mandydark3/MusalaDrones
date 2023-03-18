using Microsoft.AspNetCore.Mvc;
using MusalaDrones.Data;
using MusalaDrones.Model;
using MusalaDrones.Model.AuxiliaryModels;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using MusalaDrones.Extra;

namespace MusalaDrones.Controllers
{
    [ApiController]
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
        [Route("~/api/GetDroneBatteryLevel")]
        [HttpGet]
        public JsonResult GetDroneBatteryLevel(string searchData)
        {
            // Every Input/Output MUST be Json format, therefore we need an Auxiliary Model data type for this
            DroneSearchAM dSearch = null;
            try
            {
                dSearch = JsonSerializer.Deserialize<DroneSearchAM>(searchData);
                if (dSearch == null)
                    return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }
            catch
            {
                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }

            try
            {
                var drone = _context.Drones.FirstOrDefault(p => p.SerialNumber == dSearch.SerialNumber);
                if (drone == null)
                    return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_NOTFOUND });

                return new JsonResult(new JsonResultAM() { Result = drone.BatteryCapacity.ToString() });
            }
            catch (Exception e)
            {
                return new JsonResult(new JsonResultAM() { Result = string.Format(JsonResults.JSON_DBERROR, e.Message) });
            }
        }

        // Checking available drones for loading
        [Route("~/api/GetAvailableDronesForLoading")]
        [HttpGet]
        public JsonResult GetAvailableDronesForLoading()
        {
            try
            {
                var query = _context.Drones.Where(p => p.State == EDroneState.IDLE && p.BatteryCapacity > 25).
                                            Select(p => new
                                            {
                                                Drone = new
                                                {
                                                    ID = p.ID,
                                                    SerialNumber = p.SerialNumber,
                                                    Model = p.Model.GetDisplayName(),
                                                    WeightLimit = p.WeightLimit,
                                                    BatteryCapacity = p.BatteryCapacity
                                                }
                                            });
                var result = JsonSerializer.Serialize(query.ToList());
                return new JsonResult(new JsonResultAM() { Result = result });
            }
            catch (Exception e)
            {
                return new JsonResult(new JsonResultAM() { Result = string.Format(JsonResults.JSON_DBERROR, e.Message) });
            }
        }

        // Checking loaded medication items for a given drone
        [Route("~/api/GetMedicationFromDrone")]
        [HttpGet]
        public JsonResult GetMedicationFromDrone(string searchData)
        {
            // Every Input/Output MUST be Json format, therefore we need an Auxiliary Model data type for this
            DroneSearchAM dSearch = null;
            try
            {
                dSearch = JsonSerializer.Deserialize<DroneSearchAM>(searchData);
                if (dSearch == null)
                    return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }
            catch
            {
                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }

            try
            {
                if (!_context.Drones.Any(p => p.SerialNumber == dSearch.SerialNumber))
                    return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_NOTFOUND });

                var query = _context.DronesMedications.Include(d => d.Drone).
                                                       Include(m => m.Medication).
                                                       Where(d => d.Drone.SerialNumber == dSearch.SerialNumber).
                                                       Select(s => new { Medication = new 
                                                       { 
                                                           ID = s.Medication.ID, 
                                                           Code = s.Medication.Code, 
                                                           Name = s.Medication.Name,
                                                           Weight = s.Medication.Weight
                                                       }, Quantity = s.Quantity}).ToList();
                var result = JsonSerializer.Serialize(query);
                return new JsonResult(new JsonResultAM() { Result = result });
            }
            catch (Exception e)
            {
                return new JsonResult(new JsonResultAM() { Result = string.Format(JsonResults.JSON_DBERROR, e.Message) });
            }
        }

        // Loading a drone with medication items
        [Route("~/api/LoadDroneWithMedication")]
        [HttpPost]
        public JsonResult LoadDroneWithMedication(string loadingInfo)
        {
            // Every Input/Output MUST be Json format, therefore we need an Auxiliary Model data type for this
            DroneLoadingInfoAM linfo = null;
            try
            {
                linfo = JsonSerializer.Deserialize<DroneLoadingInfoAM>(loadingInfo);
                if (linfo == null)
                    return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }
            catch
            {
                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }

            var drone = _context.Drones.FirstOrDefault(p => p.SerialNumber == linfo.SerialNumber);
            if (drone == null)
                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_NOTFOUND });

            // Low battery?
            if (drone.BatteryCapacity <= LOW_BATTERY_THRESHOLD)
                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_LOWBATTERY });

            // Idle?
            if (drone.State != EDroneState.IDLE)
                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_NOTAVAILABLE });

            // No medications
            if (linfo.Medications == null || linfo.Medications.Count == 0)
                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_NOMEDICATIONS });

            DroneLoadingInfoAM medicationLeft = new DroneLoadingInfoAM { SerialNumber = drone.SerialNumber };
            /* Trying to load the drone with the medications. Two scenarios may occur:
             *  [a] Drone could be loaded and no medication left
             *  [b] Drone could be loaded and some medication left
             */

            try
            {
                using (var ts = new TransactionScope())
                {
                    // Drone loading
                    drone.State = EDroneState.LOADING;
                    _context.SaveChanges();

                    foreach (var med in linfo.Medications)
                    {
                        // Safe null verification
                        if (!string.IsNullOrEmpty(med.Key))
                        {
                            var medication = _context.Medications.FirstOrDefault(p => p.Code == med.Key);
                            if (medication != null)
                            {
                                float currentWeight = _context.DronesMedications.Include(d => d.Drone).
                                                                                 Include(m => m.Medication).
                                                                                 Where(dm => dm.Drone.SerialNumber == drone.SerialNumber).
                                                                                 Sum(s => s.Medication.Weight * s.Quantity);
                                // Trying to optimize the loading proccess
                                int counter = 0;
                                DroneMedication newLoadingMed = new DroneMedication
                                {
                                    Drone = drone,
                                    Medication = medication,
                                };
                                for (int iter = 0; iter < med.Value; iter++)
                                {
                                    // Medication CAN be added
                                    if (drone.WeightLimit >= currentWeight + medication.Weight * (iter + 1))
                                        counter++;
                                }
                                if (counter > 0)
                                {
                                    newLoadingMed.Quantity = counter;
                                    _context.DronesMedications.Add(newLoadingMed);
                                    _context.SaveChanges();
                                }

                                // Something left?
                                if (counter != med.Value)
                                    medicationLeft.Medications.Add(new KeyValuePair<string, int>(med.Key, med.Value - counter));
                            }
                        }
                    }

                    // Drone loaded 
                    // Assuming that the drone goes to LOADED state every time it loads medication regardless of
                    // NOT be fully loaded (Medications weight sum  == drone weight limit)
                    if (_context.DronesMedications.Include(d => d.Drone).Any(d => d.Drone.SerialNumber == drone.SerialNumber))
                        drone.State = EDroneState.LOADED;
                    else
                        // No medication could be loaded, reverting to IDLE
                        drone.State = EDroneState.IDLE;

                    _context.SaveChanges();
                    ts.Complete();

                    var result = JsonSerializer.Serialize(medicationLeft);
                    return new JsonResult(new JsonResultAM() { Result = result });
                }
            }
            catch (Exception e)
            {
                return new JsonResult(new JsonResultAM() { Result = string.Format(JsonResults.JSON_DBERROR, e.Message) });
            }
        }

        // Registering a drone
        [Route("~/api/RegisteringDrone")]
        [HttpPost]
        public JsonResult RegisteringDrone(string newDrone)
        {
            // Avoid bad deserialization
            Drone drone = null;
            try
            {
                drone = JsonSerializer.Deserialize<Drone>(newDrone);
                if (drone == null)
                    return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }
            catch
            {
                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }

            // Validate drone data
            if (string.IsNullOrEmpty(drone.SerialNumber) || drone.Model < EDroneModel.Lightweight || drone.Model > EDroneModel.Heavyweight)
                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });

            // Already exists?
            if (_context.Drones.Any(p => p.SerialNumber == drone.SerialNumber))
                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_DUPLICATEDRONE });
            
            // Using reflection to get the values of the model attributes guarantees in the future only modify the model itself to change the validations
            var fp1 = drone.GetType().GetProperty("SerialNumber");
            if (fp1 != null)
            {
                MaxLengthAttribute maxAttr = fp1.GetCustomAttributes(typeof(MaxLengthAttribute), false)[0] as MaxLengthAttribute;
                if (maxAttr != null && drone.SerialNumber.Length > maxAttr.Length)
                    return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }

            var fp2 = drone.GetType().GetProperty("WeightLimit");
            if (fp2 != null)
            {
                RangeAttribute rangeAttr = fp2.GetCustomAttributes(typeof(RangeAttribute), false)[0] as RangeAttribute;
                if (rangeAttr != null)
                {
                    float minWeight = float.MinValue;
                    var attrMin = rangeAttr.Minimum.ToString();
                    if (!string.IsNullOrEmpty(attrMin))
                        minWeight = float.Parse(attrMin);

                    float maxWeight = float.MaxValue;
                    var attrMax = rangeAttr.Maximum.ToString();
                    if (!string.IsNullOrEmpty(attrMax))
                        maxWeight = float.Parse(attrMax);

                    if (drone.WeightLimit < minWeight || drone.WeightLimit > maxWeight)
                        return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
                }
            }

            // Percent
            drone.BatteryCapacity = Math.Clamp(drone.BatteryCapacity, 0, 100);
            // Default state is IDLE
            drone.State = EDroneState.IDLE;

            try
            {
                _context.Drones.Add(drone);
                _context.SaveChanges();

                return new JsonResult(new JsonResultAM() { Result = JsonResults.JSON_OK });
            }
            catch (Exception e)
            {
                return new JsonResult(new JsonResultAM() { Result = string.Format(JsonResults.JSON_DBERROR, e.Message) });
            }
        }
    }
}
