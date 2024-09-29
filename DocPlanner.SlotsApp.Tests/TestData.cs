using DocPlanner.SlotsApp.Domain;
using NodaTime;

namespace DocPlanner.SlotsApp.Tests;

public static class TestData
{
    public static Availability SampleAvailability()
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

        return availability;
    }
}
