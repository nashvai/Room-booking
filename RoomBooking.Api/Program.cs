using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using RoomBooking.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Register controllers and serialize enums as strings.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// Existing OpenAPI configuration.
builder.Services.AddOpenApi();

// Register the in-memory database.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("RoomBookingDb"));

var app = builder.Build();

// Seed the database.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    DbSeeder.Seed(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controller routes.
app.MapControllers();

// Existing weather forecast endpoint.
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF =>
        32 + (int)(TemperatureC / 0.5556);
}
