using DocPlanner.SlotsApp.Domain;
using NodaTime;

namespace DocPlanner.SlotsApp.Tests.Unit.Domain;

public class AppointmentTests
{
    private static readonly Patient aPatient = new("John Doe", "blah", "@mail.com", "1234");

    [Fact]
    public void CanCreate_Appointment()
    {
        // Arrange
        var date = new LocalDate(2024, 9, 24);
        var start = new LocalTime(10, 0);
        var end = new LocalTime(12, 0);

        // Act
        var slot = new Appointment(date, start, end, "Checkup")
        {
            Patient = aPatient
        };

        // Assert
        Assert.Equal(start, slot.Start);
        Assert.Equal(end, slot.End);
        Assert.Equal("Checkup", slot.Comments);
        Assert.Equal(aPatient, slot.Patient);
    }

    [Fact]
    public void StartDate_ShouldBeGreater_ThanEnd()
    {
        // Arrange
        var date = new LocalDate(2024, 9, 24);
        var start = new LocalTime(12, 0);
        var end = new LocalTime(10, 0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            var slot = new Appointment(date, start, end, "Checkup")
            {
                Patient = aPatient
            };
        });
    }
}