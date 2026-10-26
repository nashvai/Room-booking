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

app.Run();


