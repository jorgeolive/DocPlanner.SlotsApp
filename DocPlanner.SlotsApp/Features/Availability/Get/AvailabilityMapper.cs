using DocPlanner.SlotsApp.Domain;
using DocPlanner.SlotsApp.Features.Availability.Get;
using NodaTime;

public static class AvailabilityMapper
{
    public static WeeklyAvailabilityModel MapToWeeklyAvailabilityModel(Availability availability)
    {
        var facility = new FacilityModel(availability.Facility.Name, availability.Facility.Address);
        int slotDurationInMinutes = availability.SlotDurationMinutes;

        var workPeriodsDict = availability.WorkPeriods.ToDictionary(wp => wp.DayOfWeek);

        WorkDayModel monday = MapWorkDay(workPeriodsDict, IsoDayOfWeek.Monday, availability.Appointments);
        WorkDayModel tuesday = MapWorkDay(workPeriodsDict, IsoDayOfWeek.Tuesday, availability.Appointments);
        WorkDayModel wednesday = MapWorkDay(workPeriodsDict, IsoDayOfWeek.Wednesday, availability.Appointments);
        WorkDayModel thursday = MapWorkDay(workPeriodsDict, IsoDayOfWeek.Thursday, availability.Appointments);
        WorkDayModel friday = MapWorkDay(workPeriodsDict, IsoDayOfWeek.Friday, availability.Appointments);
        WorkDayModel saturday = MapWorkDay(workPeriodsDict, IsoDayOfWeek.Saturday, availability.Appointments);
        WorkDayModel sunday = MapWorkDay(workPeriodsDict, IsoDayOfWeek.Sunday, availability.Appointments);

        return new WeeklyAvailabilityModel(facility, slotDurationInMinutes, monday, tuesday, wednesday, thursday, friday, saturday, sunday);
    }

    private static WorkDayModel MapWorkDay(
        Dictionary<IsoDayOfWeek, WorkPeriod> workPeriodsDict,
        IsoDayOfWeek dayOfWeek,
        IReadOnlyList<Appointment> appointments)
    {
        if (workPeriodsDict.TryGetValue(dayOfWeek, out var workPeriod))
        {
            var busySlots = appointments
                .Where(a => a.Date.DayOfWeek == dayOfWeek)
                .Select(a => new BusySlotModel(
                    new DateTime(a.Date.Year, a.Date.Month, a.Date.Day, a.Start.Hour, a.Start.Minute, 0),
                    new DateTime(a.Date.Year, a.Date.Month, a.Date.Day, a.End.Hour, a.End.Minute, 0)))
                .ToList();

            var mappedWorkPeriod = new WorkPeriodModel(
                workPeriod.OffHours[0].End.Hour,
                workPeriod.LunchBreak.Start.Hour,
                workPeriod.LunchBreak.End.Hour,
                workPeriod.OffHours[1].Start.Hour,
                busySlots
            );

            return new WorkDayModel(mappedWorkPeriod);
        }

        return new WorkDayModel(new WorkPeriodModel(0, 0, 0, 0, Enumerable.Empty<BusySlotModel>()));
    }
}
