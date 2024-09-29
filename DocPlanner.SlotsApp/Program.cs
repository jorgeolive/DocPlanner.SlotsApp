using DocPlanner.SlotsApp;
using DocPlanner.SlotsApp.Domain;
using DocPlanner.SlotsApp.Host;
using DocPlanner.SlotsApp.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.OpenApi.Models;
using NodaTime;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic", // Make sure this is lowercase
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Basic Authentication header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<SlotsAppDbContext>(options =>
    options.UseInMemoryDatabase("appDb"));

builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(AssemblyMarkerClass).Assembly); });

var app = builder.Build();

SeedDatabase(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.RegisterSlotAppEndpoints();

app.Run();

void SeedDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<SlotsAppDbContext>();

        context.Database.EnsureCreated();

        if (!context.Availabilities.Any())
        {
            var facility = new Facility { Name = "Main Clinic", Address = "My Clinic" };
            var workPeriods = new List<WorkPeriod>
            {
                WorkPeriod.Create(
                    IsoDayOfWeek.Monday, 10, 18, 13, 14),
                WorkPeriod.Create(
                    IsoDayOfWeek.Tuesday, 10, 18, 13, 14),
                WorkPeriod.Create(
                    IsoDayOfWeek.Wednesday, 10, 18, 13, 14),
                WorkPeriod.Create(
                    IsoDayOfWeek.Thursday, 10, 18, 13, 14),
                WorkPeriod.Create(
                    IsoDayOfWeek.Friday, 10, 18, 13, 14)
            };

            var availability = new Availability { Facility = facility, SlotDurationMinutes = 30, WorkPeriods = workPeriods };

            availability.AddAppointment(
                new Appointment
                (
                    LocalDate.FromDateTime(new DateTime(2024, 9, 25)),
                    LocalTime.FromHourMinuteSecondTick(10, 0, 0, 0),
                    LocalTime.FromHourMinuteSecondTick(10, 30, 0, 0),
                    "I have a headache because this code test is too long. Did you know that I participated writing DocPlanner's CalendarApp? :)")
                {
                    Patient = new Patient("Jorge", "Olive", "my@mail.com", "241412414")
                });

            context.Availabilities.Add(availability);

            context.SaveChanges();
        }
    }
}

public partial class Program { }