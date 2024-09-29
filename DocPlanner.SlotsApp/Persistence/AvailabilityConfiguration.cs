using DocPlanner.SlotsApp.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;
using System.Globalization;

namespace DocPlanner.SlotsApp.Persistence
{
    public class AvailabilityConfiguration : IEntityTypeConfiguration<Availability>
    {
        public void Configure(EntityTypeBuilder<Availability> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedNever();

            builder.Property(a => a.SlotDurationMinutes)
                .IsRequired();

            builder.HasMany<Appointment>()
                .WithOne()
                .HasForeignKey(x => x.AvailabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(a => a.Facility);

            builder.OwnsMany(a => a.WorkPeriods, wp =>
            {
                wp.WithOwner();

                wp.Property(w => w.DayOfWeek).IsRequired();
                wp.HasKey(wp => wp.DayOfWeek);

                wp.Property(wp => wp.DayOfWeek)
                       .HasConversion<int>()
                       .IsRequired();

                wp.OwnsOne(wp => wp.LunchBreak, lb =>
                {
                    lb.Property(o => o.Start)
                      .HasConversion(
                          localTime => localTime.ToString("HH:mm", CultureInfo.InvariantCulture), // Convert to string
                          str => CreateLocalTime(str))
                      .IsRequired();

                    lb.Property(o => o.End)
                      .HasConversion(
                          localTime => localTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                          str => CreateLocalTime(str))
                      .IsRequired();
                });

                wp.OwnsMany(wp => wp.OffHours, offHours =>
                {
                    offHours.WithOwner().HasForeignKey("WorkPeriodId");
                    offHours.Property(o => o.Start)
                            .HasConversion(
                                localTime => localTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                                str => CreateLocalTime(str))
                            .IsRequired();

                    offHours.Property(o => o.End)
                            .HasConversion(
                                localTime => localTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                                str => CreateLocalTime(str))
                            .IsRequired();
                });


            });
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
}