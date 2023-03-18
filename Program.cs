using Microsoft.EntityFrameworkCore;
using MusalaDrones.Data;
using MusalaDrones.Extra;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<MusalaDronesSeeder>();
builder.Services.AddDbContext<MusalaDronesDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Register the periodic task service
builder.Services.AddSingleton<TaskService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<TaskService>());

var app = builder.Build();
app.MapGet("/", () => "App is running!");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MusalaDronesDbContext>();
    db.Database.Migrate();
    // For testing and debugging
    db.Seed();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
