using NodaTime;

namespace DocPlanner.SlotsApp.Domain;

public record WorkPeriod
{
    public IsoDayOfWeek DayOfWeek { get; private init; } = default!;
    public OffHours LunchBreak { get; private init; } = default!;
    public List<OffHours> OffHours { get; private init; } = default!;

    private WorkPeriod()
    {
    }

    public static WorkPeriod Create(IsoDayOfWeek dayOfWeek, int startHour, int endHour, int lunchStartHour, int lunchEndHour)
    {
        try
        {
            List<OffHours> workOffHours = new List<OffHours>();

            workOffHours.AddRange([
                new OffHours(new LocalTime(0, 0), new LocalTime(startHour, 0)),
                new OffHours(new LocalTime(endHour, 0), LocalTime.MaxValue)
            ]);

            var lunchBreak = new OffHours(new LocalTime(lunchStartHour, 0), new LocalTime(lunchEndHour, 0));

            if (lunchBreak.Start < workOffHours[0].End || lunchBreak.End > workOffHours[1].Start)
            {
                throw new InvalidWorkperiodConfigurationException();
            }

            return new WorkPeriod
            {
                DayOfWeek = dayOfWeek,
                LunchBreak = lunchBreak,
                OffHours = workOffHours
            };
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new InvalidWorkperiodConfigurationException(ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidWorkperiodConfigurationException(ex.Message);
        }
    }

    public int StartHour => OffHours[0].End.Hour;
    public int EndHour => OffHours[1].Start.Hour;
}
