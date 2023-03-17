using Microsoft.AspNetCore.Mvc;
using MusalaDrones.Data;

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


        // Check drone battery level for a given drone
        [HttpGet(Name = "GetDroneBatteryLevel")]
        public JsonResult GetDroneBatteryLevel(string droneSN)
        {
            var drone = _context.Drones.FirstOrDefault(p => p.SerialNumber == droneSN);
            if (drone == null)
                return new JsonResult(JsonResults.JSON_NOTFOUND);

            return new JsonResult(drone.BatteryCapacity);
        }
    }
}
