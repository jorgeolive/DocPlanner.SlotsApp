using DocPlanner.SlotsApp.Domain;
using FluentAssertions;
using NodaTime;

namespace DocPlanner.SlotsApp.Tests.Unit.Domain;

public class AvailabilityTests
{
    private static readonly Patient aPatient = new("John Doe", "blah", "@mail.com", "1234");
    private static readonly Facility aFacility = new() { Address = "Address", Name = "Facility" };

    [Fact]
    public void CanAddMultipleNonOverlappingAppointments()
    {
        // Arrange
        var workPeriods = new List<WorkPeriod>
        {
            WorkPeriod.Create(IsoDayOfWeek.Monday, 9, 17, 12, 13) // Work period from 9 AM to 5 PM, with lunch break from 12 PM to 1 PM
        };

        var availability = new Availability { Facility = aFacility, SlotDurationMinutes = 60, WorkPeriods = workPeriods };

        var monday9thSept = new LocalDate(2024, 9, 23);

        // Create the first slot (from 9:00 AM to 10:00 AM)
        var slot1 = new Appointment(
            monday9thSept,
            new LocalTime(9, 0),
            new LocalTime(10, 0),
            "Consultation")
        {
            Patient = aPatient
        };

        // Create the second slot (from 10:00 AM to 11:00 AM)
        var slot2 = new Appointment(
            monday9thSept,
            new LocalTime(10, 0),
            new LocalTime(11, 0),
            "Follow-up")
        {
            Patient = aPatient
        };

        // Create the third slot (from 1:00 PM to 2:00 PM, after lunch break)
        var slot3 = new Appointment(
            monday9thSept,
            new LocalTime(13, 0),
            new LocalTime(14, 0),
            "Checkup")
        {
            Patient = aPatient
        };

        // Act: Add all the slots to availability
        availability.AddAppointment(slot1);
        availability.AddAppointment(slot2);
        availability.AddAppointment(slot3);

        // Assert: Ensure that all 3 slots have been added without issues
        availability.Appointments.Should().HaveCount(3);
        availability.Appointments.Should().Contain(new[] { slot1, slot2, slot3 });
    }

    [Fact]
    public void CantCreateAvailability_WithDuplicateDaysOfWeek()
    {
        Action newAvailability = () => new Availability
        {
            Facility = aFacility,
            SlotDurationMinutes = 30,
            WorkPeriods =
                [
                    WorkPeriod.Create(IsoDayOfWeek.Monday, 8, 16, 12, 13),
                    WorkPeriod.Create(IsoDayOfWeek.Monday, 8, 16, 12, 13),
                    WorkPeriod.Create(IsoDayOfWeek.Tuesday, 8, 16, 12, 13)
                ]
        };

        newAvailability.Should().Throw<ArgumentException>().WithMessage("There must be at most one work period for each day of the week.");
    }

    [Theory]
    [InlineData(8, 9, 9, 17)]  // Slot starts before work period
    [InlineData(16, 18, 9, 17)] // Slot ends after work period
    [InlineData(9, 18, 9, 17)] // Slot completely outside work period
    public void AddAppointment_WithinWorkPeriod_ShouldValidateWorkHours(int startHour, int endHour, int workPeriodStart, int workPeriodEnd)
    {
        // Arrange
        var workPeriods = new List<WorkPeriod>
            {
                WorkPeriod.Create(IsoDayOfWeek.Monday, workPeriodStart, workPeriodEnd, 12, 13)
            };
        var availability = new Availability { Facility = aFacility, SlotDurationMinutes = 60, WorkPeriods = workPeriods };
        var start = new LocalTime(startHour, 0);
        var end = new LocalTime(endHour, 0);
        var appointment = new Appointment(new(2024, 9, 30), start, end, "Consultation")
        {
            Patient = aPatient
        };

        Action addAppointment = () => availability.AddAppointment(appointment);

        // Act & Assert
        addAppointment.Should().Throw<InvalidSlotException>()
            .WithMessage("OccupiedSlot must be within the work period hours.");
    }

    [Theory]
    [InlineData(10, 11, 10, 11, false)]  // Overlapping slots (exactly the same time)
    [InlineData(10, 11, 11, 12, true)]   // Non-overlapping slots (adjacent)
    public void AddAppointment_ShouldValidateOverlap(int slot1StartHour, int slot1EndHour, int slot2StartHour, int slot2EndHour, bool shouldSucceed)
    {
        // Arrange
        var workPeriods = new List<WorkPeriod>
            {
                WorkPeriod.Create(IsoDayOfWeek.Monday, 9, 17, 12, 13) // Work period from 9 AM to 5 PM
            };

        var availability = new Availability { Facility = aFacility, SlotDurationMinutes = 60, WorkPeriods = workPeriods };

        var monday9thSept = new LocalDate(2024, 9, 23);

        // Create the first slot (from slot1StartHour to slot1EndHour)
        var slot1 = new Appointment(
            monday9thSept,
            new LocalTime(slot1StartHour, 0),
            new LocalTime(slot1EndHour, 0),
            "Consultation")
        {
            Patient = aPatient
        };

        // Create the second slot (from slot2StartHour to slot2EndHour)
        var slot2 = new Appointment(
            monday9thSept,
            new LocalTime(slot2StartHour, 0),
            new LocalTime(slot2EndHour, 0),
            "Follow-up")
        {
            Patient = aPatient
        };
        // Add the first slot to the availability
        availability.AddAppointment(slot1);

        if (shouldSucceed)
        {
            // Act: Try adding the second slot
            availability.AddAppointment(slot2);

            // Assert: Two slots should be added if there's no overlap
            Assert.Equal(2, availability.Appointments.Count);
        }
        else
        {
            // Act & Assert: Should throw an exception if the slots overlap
            Assert.Throws<InvalidSlotException>(() => availability.AddAppointment(slot2))
                .Message.Should().Be("The new occupied slot overlaps with an existing slot.");
        }
    }

    [Theory]
    [InlineData(9, 13, 60)]   // Invalid slot duration (4 hours)
    [InlineData(14, 15, 90)]  // Invalid slot duration (60 minutes)
    [InlineData(9, 11, 60)]  // Invalid slot duration (120 minutes)
    [InlineData(9, 14, 120)]  // Invalid slot duration (120 minutes)
    public void CantAddAppointment_OfInvalidDuration(int appointmentStartHour, int appointmentEndHour, int facilitySlotDuration)
    {
        var workPeriods = new List<WorkPeriod>
            {
                WorkPeriod.Create(IsoDayOfWeek.Monday, 9, 17, 12, 13)
            };
        var availability = new Availability { Facility = aFacility, SlotDurationMinutes = facilitySlotDuration, WorkPeriods = workPeriods };

        var monday9thSept = new LocalDate(2024, 9, 23);
        var start = new LocalTime(appointmentStartHour, 0);
        var end = new LocalTime(appointmentEndHour, 0);
        var slot = new Appointment(monday9thSept, start, end, "Consultation")
        {
            Patient = aPatient
        };

        Assert.Throws<InvalidSlotException>(() => availability.AddAppointment(slot));
    }
}