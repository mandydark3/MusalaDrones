using MusalaDrones.Data;
using MusalaDrones.Model;

namespace MusalaDrones.Extra
{
    public class TaskService : BackgroundService
    {
        private readonly ILogger<TaskService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TimeSpan _timer = TimeSpan.FromMinutes(5);

        public TaskService(ILogger<TaskService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_timer);
            while (!stoppingToken.IsCancellationRequested &&
                    await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<MusalaDronesDbContext>();
                    if (context == null)
                    {
                        List<Log> logs = new List<Log>();
                        foreach (var drone in context.Drones)
                        {
                            Log log = new Log
                            {
                                Drone = drone,
                                BatterLevel = drone.BatteryCapacity,
                                Checked = DateTime.Now
                            };
                            logs.Add(log);
                        }
                        context.Logs.AddRange(logs);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Task service failed: {ex.Message}.");
                }
            }
        }
    }
}
