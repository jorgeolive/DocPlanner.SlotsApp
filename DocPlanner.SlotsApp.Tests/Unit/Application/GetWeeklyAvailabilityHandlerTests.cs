using DocPlanner.SlotsApp.Features.Availability.Get;
using DocPlanner.SlotsApp.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DocPlanner.SlotsApp.Tests;

public class GetWeeklyAvailabilityHandlerTests : IDisposable
{
    private readonly DbContextOptions<SlotsAppDbContext> _dbContextOptions;
    private readonly SlotsAppDbContext _dbContext;

    public GetWeeklyAvailabilityHandlerTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<SlotsAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new SlotsAppDbContext(_dbContextOptions);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidGetWeeklyAvailabilityDateException_WhenDateIsNotMonday()
    {
        // Arrange
        var handler = new GetWeeklyAvailabilityHander(_dbContext);

        var request = new GetWeeklyAvailabilityRequest(DateOnly.Parse("2024-09-25"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidGetWeeklyAvailabilityDateException>(
            () => handler.Handle(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnWeeklyAvailability_WhenDateIsMonday()
    {
        SeedDatabase();
        var handler = new GetWeeklyAvailabilityHander(_dbContext);

        var request = new GetWeeklyAvailabilityRequest(DateOnly.Parse("2024-09-23"));

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Main Clinic", result.Facility.Name);
        Assert.Equal(10, result.Monday.Workperiod.StartHour);
    }

    private void SeedDatabase()
    {
        var availability = TestData.SampleAvailability();
        _dbContext.Availabilities.Add(availability);
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
