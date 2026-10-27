using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using RoomBooking.Api.Data;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Options;
using RoomBooking.Api.Validation;
using RoomBooking.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers and consistent request-validation errors.
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var message = context.ModelState
                .Values
                .SelectMany(value => value.Errors)
                .Select(error =>
                    !string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? error.ErrorMessage
                        : "The supplied value is invalid.")
                .FirstOrDefault()
                ?? "The request is invalid.";

            return new BadRequestObjectResult(
                new ApiError(
                    ErrorCodes.InvalidRequest,
                    message));
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// In-memory database.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("RoomBookingDb"));

// Booking configuration.
builder.Services.Configure<BookingRulesOptions>(
    builder.Configuration.GetSection(
        BookingRulesOptions.SectionName));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Clock.
builder.Services.AddSingleton(TimeProvider.System);


// Booking validation rules.
builder.Services.AddScoped<IBookingRule, TimeRangeRule>();
builder.Services.AddScoped<IBookingRule, BookingHorizonRule>();
builder.Services.AddScoped<IBookingRule, CapacityRule>();
builder.Services.AddScoped<IBookingRule, MaxDurationRule>();
builder.Services.AddScoped<IBookingRule, ActiveRoomRule>();
builder.Services.AddScoped<IBookingRule, RoomOverlapRule>();
builder.Services.AddScoped<IBookingRule, OrganizerOverlapRule>();

// Validation service.
builder.Services.AddScoped<BookingValidationService>();
builder.Services.AddScoped<UsageReportService>();


var app = builder.Build();

// Seed the database.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    DbSeeder.Seed(db);
}

app.UseCors("Frontend");

// OpenAPI.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controller routes.
app.MapControllers();

app.Run();
