using DocPlanner.SlotsApp.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;
using System.Globalization;

namespace DocPlanner.SlotsApp.Persistence;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.Date)
            .IsRequired();

        builder.OwnsOne(x => x.Patient);

        builder.Property(a => a.Start)
            .HasConversion(
                          localTime => localTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                          str => CreateLocalTime(str))
            .IsRequired();

        builder.Property(a => a.End)
            .HasConversion(
                          localTime => localTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                          str => CreateLocalTime(str))
            .IsRequired();
    }
    private static LocalTime CreateLocalTime(string time)
    {
        var parts = time.Split(':');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int hour) &&
            int.TryParse(parts[1], out int minute))
        {
            return new LocalTime(hour, minute);
        }
        throw new FormatException("Invalid time format");
    }
}
