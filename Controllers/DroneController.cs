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

        private readonly MusalaDronesDbContext _context;

        public DroneController(MusalaDronesDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checking drone battery level for a given drone
        /// </summary>
        /// <param name="searchData">
        /// JSON param formed as { "SerialNumber": [Drone serial number] }
        /// </param>
        /// <returns>
        /// JSON result formed as: { "result": [The actual result] }
        /// Actual result could be the drone battery level or if an error occurred 
        /// </returns>
        [Route("~/api/GetDroneBatteryLevel")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JsonResultAM))]
        [HttpGet]
        public IActionResult GetDroneBatteryLevel(string searchData)
        {
            // Every Input/Output MUST be Json format, therefore we need an Auxiliary Model data type for this
            DroneSearchAM dSearch = null;
            try
            {
                dSearch = JsonSerializer.Deserialize<DroneSearchAM>(searchData);
                if (dSearch == null)
                    return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }
            catch
            {
                return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }

            try
            {
                var drone = _context.Drones.SingleOrDefault(p => p.SerialNumber == dSearch.SerialNumber);
                if (drone == null)
                    return new NotFoundResult();

                return Ok(new JsonResultAM() { Result = drone.BatteryCapacity.ToString() });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new JsonResultAM() { Result = string.Format(JsonResults.JSON_DBERROR, e.Message) });
            }
        }

        /// <summary>
        /// Checking available drones for loading
        /// </summary>
        /// <returns>
        /// JSON result formed as: { "result": [The actual result] }
        /// Actual result could be the list containing available drones or if an error occurred 
        /// </returns>
        [Route("~/api/GetAvailableDronesForLoading")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JsonResultAM))]
        [HttpGet]
        public IActionResult GetAvailableDronesForLoading()
        {
            try
            {
                var query = _context.Drones.Where(p => p.State == EDroneState.IDLE && p.BatteryCapacity > 25).
                                            Select(p => new
                                            {
                                                Drone = new
                                                {
                                                    SerialNumber = p.SerialNumber,
                                                    Model = p.Model.GetDisplayName(),
                                                    WeightLimit = p.WeightLimit,
                                                    BatteryCapacity = p.BatteryCapacity
                                                }
                                            });
                var result = JsonSerializer.Serialize(query.ToList());
                return Ok(new JsonResultAM() { Result = result });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new JsonResultAM() { Result = string.Format(JsonResults.JSON_DBERROR, e.Message) });
            }
        }

        /// <summary>
        /// Checking loaded medication items for a given drone
        /// </summary>
        /// <param name="searchData">
        /// JSON param formed as { "SerialNumber": [Drone serial number] }
        /// </param>
        /// <returns>
        /// JSON result formed as: { "result": [The actual result] }
        /// Actual result could be the list containing medication loaded into the drone or if an error occurred 
        /// </returns>
        [Route("~/api/GetMedicationFromDrone")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JsonResultAM))]
        [HttpGet]
        public IActionResult GetMedicationFromDrone(string searchData)
        {
            // Every Input/Output MUST be Json format, therefore we need an Auxiliary Model data type for this
            DroneSearchAM dSearch = null;
            try
            {
                dSearch = JsonSerializer.Deserialize<DroneSearchAM>(searchData);
                if (dSearch == null)
                    return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }
            catch
            {
                return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }

            try
            {
                var drone = _context.Drones.SingleOrDefault(p => p.SerialNumber == dSearch.SerialNumber);
                if (drone == null)
                    return new NotFoundResult();

                var query = _context.DronesMedications.Include(m => m.Medication).
                                                       Where(d => d.DroneID == drone.ID).
                                                       Select(s => new { Medication = new 
                                                       { 
                                                           Code = s.Medication.Code, 
                                                           Name = s.Medication.Name,
                                                           Weight = s.Medication.Weight
                                                       }, Quantity = s.Quantity}).ToList();
                var result = JsonSerializer.Serialize(query);
                return Ok(new JsonResultAM() { Result = result });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new JsonResultAM() { Result = string.Format(JsonResults.JSON_DBERROR, e.Message) });
            }
        }

        /// <summary>
        /// Loading a drone with medication items
        /// </summary>
        /// <param name="loadingInfo">
        /// JSON param formed as { "SerialNumber": [Drone serial number], "Medications": [Pair key-value list formead as "Key": "Code", "Value": Quantity] }
        /// </param>
        /// <returns>
        /// JSON result formed as: { "result": [The actual result] }
        /// Actual result could be the list containing medication left, if any (couldn't be loaded into the drone due to drone weight limit) or if an error occurred 
        /// </returns>
        [Route("~/api/LoadDroneWithMedication")]
        [ProducesResponseType(StatusCodes.Status303SeeOther)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JsonResultAM))]
        [HttpPost]
        public IActionResult LoadDroneWithMedication(string loadingInfo)
        {
            // Every Input/Output MUST be Json format, therefore we need an Auxiliary Model data type for this
            DroneLoadingInfoAM linfo = null;
            try
            {
                linfo = JsonSerializer.Deserialize<DroneLoadingInfoAM>(loadingInfo);
                if (linfo == null)
                    return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }
            catch
            {
                return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGLOADINGINFO });
            }

            var drone = _context.Drones.SingleOrDefault(p => p.SerialNumber == linfo.SerialNumber);
            if (drone == null)
                return new NotFoundResult();

            // Low battery?
            if (drone.BatteryCapacity <= LOW_BATTERY_THRESHOLD)
                return StatusCode(StatusCodes.Status303SeeOther, new JsonResultAM() { Result = JsonResults.JSON_LOWBATTERY });

            // Idle?
            if (drone.State != EDroneState.IDLE)
                return StatusCode(StatusCodes.Status303SeeOther, new JsonResultAM() { Result = JsonResults.JSON_NOTAVAILABLE });

            // No medications
            if (linfo.Medications == null || linfo.Medications.Count == 0)
                return StatusCode(StatusCodes.Status303SeeOther, new JsonResultAM() { Result = JsonResults.JSON_NOMEDICATIONS });

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

                    float currentWeight = 0;
                    foreach (var med in linfo.Medications)
                    {
                        // Safe null verification
                        if (!string.IsNullOrEmpty(med.Key))
                        {
                            var medication = _context.Medications.SingleOrDefault(p => p.Code == med.Key);
                            if (medication != null)
                            {
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
                                    currentWeight += newLoadingMed.Medication.Weight * counter;
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
                    // Assuming that the drone goes into LOADED state every time it loads medication regardless of
                    // NOT be fully loaded (Medications weight sum  == drone weight limit)
                    if (_context.DronesMedications.Include(d => d.Drone).Any(d => d.DroneID == drone.ID))
                        drone.State = EDroneState.LOADED;
                    else
                        // No medication could be loaded, reverting to IDLE
                        drone.State = EDroneState.IDLE;

                    _context.SaveChanges();
                    ts.Complete();

                    var result = JsonSerializer.Serialize(medicationLeft);
                    return Ok(new JsonResultAM() { Result = result });
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new JsonResultAM() { Result = string.Format(JsonResults.JSON_DBERROR, e.Message) });
            }
        }

        /// <summary>
        /// Registering a new drone
        /// </summary>
        /// <param name="newDrone">
        /// JSON param formed as { "SerialNumber": [Drone serial number], "Model": [Number between 1 and 4], "WeightLimit": [Weight limit], "BatteryCapacity": [Battery percent] }
        /// </param>
        /// <returns>
        /// JSON result formed as: { "result": [The actual result] }
        /// Actual result could be "OK" if the drone could be created or if an error occurred 
        /// </returns>
        [Route("~/api/RegisterDrone")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JsonResultAM))]
        [HttpPost]
        public IActionResult RegisterDrone(string newDrone)
        {
            // Avoid bad deserialization
            Drone drone = null;
            try
            {
                drone = JsonSerializer.Deserialize<Drone>(newDrone);
                if (drone == null)
                    return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGDRONEINFO });
            }
            catch
            {
                return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGDRONEINFO });
            }

            // Validate drone data
            if (string.IsNullOrEmpty(drone.SerialNumber) || drone.Model < EDroneModel.Lightweight || drone.Model > EDroneModel.Heavyweight)
                return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGDRONEINFO });

            // Already exists?
            if (_context.Drones.Any(p => p.SerialNumber == drone.SerialNumber))
                return StatusCode(StatusCodes.Status303SeeOther, new JsonResultAM() { Result = JsonResults.JSON_DUPLICATEDRONE });
            
            // Using reflection to get the values of the model attributes guarantees in the future only modify the model itself to change the validations
            var fp1 = drone.GetType().GetProperty("SerialNumber");
            if (fp1 != null)
            {
                MaxLengthAttribute maxAttr = fp1.GetCustomAttributes(typeof(MaxLengthAttribute), false)[0] as MaxLengthAttribute;
                if (maxAttr != null && drone.SerialNumber.Length > maxAttr.Length)
                    return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGDRONEINFO });
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
                        return BadRequest(new JsonResultAM() { Result = JsonResults.JSON_WRONGDRONEINFO });
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

                return Ok(new JsonResultAM() { Result = JsonResults.JSON_OK });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new JsonResultAM() { Result = string.Format(JsonResults.JSON_DBERROR, e.Message) });
            }
        }
    }
}
