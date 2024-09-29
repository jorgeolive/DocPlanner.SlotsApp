using DocPlanner.SlotsApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace DocPlanner.SlotsApp.Persistence;

public class SlotsAppDbContext : DbContext
{
    public DbSet<Availability> Availabilities { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public SlotsAppDbContext(DbContextOptions<SlotsAppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SlotsAppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}