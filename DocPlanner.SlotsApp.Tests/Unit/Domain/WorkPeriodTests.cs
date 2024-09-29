using DocPlanner.SlotsApp.Domain;
using NodaTime;

namespace DocPlanner.SlotsApp.Tests.Unit.Domain;

public class WorkPeriodTests
{
    [Theory]
    [InlineData(IsoDayOfWeek.Monday, 9, 17, 12, 13)]
    [InlineData(IsoDayOfWeek.Tuesday, 8, 16, 11, 12)]
    [InlineData(IsoDayOfWeek.Wednesday, 7, 15, 10, 11)]
    public void CanCreate_ValidWorkperiod(IsoDayOfWeek dayOfWeek, int startHour, int endHour, int lunchStartHour, int lunchEndHour)
    {
        // Arrange & Act
        var workPeriod = WorkPeriod.Create(dayOfWeek, startHour, endHour, lunchStartHour, lunchEndHour);

        // Assert
        Assert.Equal(dayOfWeek, workPeriod.DayOfWeek);
        Assert.Equal(startHour, workPeriod.StartHour);
        Assert.Equal(endHour, workPeriod.EndHour);
        Assert.Equal(lunchStartHour, workPeriod.LunchBreak.Start.Hour);
        Assert.Equal(lunchEndHour, workPeriod.LunchBreak.End.Hour);
    }

    [Theory]
    [InlineData(17, 9)]  // StartHour >= EndHour
    [InlineData(10, 10)]  // StartHour == EndHour
    public void CantCreateWith_InvalidStartAndEndHour(int startHour, int endHour)
    {
        // Arrange, Act & Assert
        Assert
            .Throws<InvalidWorkperiodConfigurationException>(() =>
        {
            WorkPeriod.Create(IsoDayOfWeek.Tuesday, startHour, endHour, 12, 13);
        });
    }

    [Theory]
    [InlineData(9, 17, 8, 13)]  // LunchStartHour before StartHour
    [InlineData(9, 17, 17, 18)] // LunchStartHour >= EndHour
    [InlineData(0, 23, 0, 1)]   // Incorrect lunch on off hours
    [InlineData(9, 17, 12, 18)]  // LunchEndHour > EndHour
    [InlineData(9, 17, 12, 11)]  // LunchEndHour <= LunchStartHour
    public void CantCreate_WithLunchNotBetweenOnHours(int startHour, int endHour, int lunchStartHour, int lunchEndHour)
    {
        // Arrange, Act & Assert
        Assert.Throws<InvalidWorkperiodConfigurationException>(() =>
        {
            WorkPeriod.Create(IsoDayOfWeek.Wednesday, startHour, endHour, lunchStartHour, lunchEndHour);
        });
    }

    [Theory]
    [InlineData(-1, 17, 12, 13)]  // StartHour < 0
    [InlineData(9, 25, 12, 13)]   // EndHour > 23
    [InlineData(9, 17, 12, 24)]   // LunchEndHour > 23
    public void CantCreate_WithHoursOutOfRange(int startHour, int endHour, int lunchStartHour, int lunchEndHour)
    {
        // Arrange, Act & Assert
        Assert.Throws<InvalidWorkperiodConfigurationException>(() =>
        {
            WorkPeriod.Create(IsoDayOfWeek.Friday, startHour, endHour, lunchStartHour, lunchEndHour);
        });
    }
}