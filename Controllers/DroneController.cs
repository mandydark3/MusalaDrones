using Microsoft.AspNetCore.Mvc;
using MusalaDrones.Data;
using MusalaDrones.Model;
using System.Text.Json;

namespace MusalaDrones.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DroneController : ControllerBase
    {
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
            List<Drone> availableList = _context.Drones.Where(p => p.State == EDroneState.IDLE && p.BatteryCapacity > 25).ToList();

            return new JsonResult(JsonSerializer.Serialize(availableList));
        }
    }
}
