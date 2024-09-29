using DocPlanner.SlotsApp.Domain;
using DocPlanner.SlotsApp.Features.Slots;
using DocPlanner.SlotsApp.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DocPlanner.SlotsApp.Tests;

public class TakeSlotHandlerTests : IDisposable
{
    private readonly DbContextOptions<SlotsAppDbContext> _dbContextOptions;
    private SlotsAppDbContext _dbContext;

    public TakeSlotHandlerTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<SlotsAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new SlotsAppDbContext(_dbContextOptions);
        SeedDatabase(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldAddAppointment_WhenRequestIsValid()
    {
        // Arrange
        var handler = new TakeSlotHandler(_dbContext);
        var request = new TakeSlotRequest(
            Start: new DateTime(2024, 9, 25, 11, 0, 0),
            End: new DateTime(2024, 9, 25, 11, 30, 0),
            Comments: "Another test appointment",
            Patient: new TakeSlotPatientModel("John", "Doe", "john.doe@example.com", "123456789")
        );

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var availability = await _dbContext.Availabilities.Include(x => x.Appointments).FirstAsync();
        Assert.Equal(2, availability.Appointments.Count);
        Assert.Equal("Another test appointment", availability.Appointments[1].Comments);
        Assert.Equal("John", availability.Appointments[1].Patient.Name);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidSlotException_WhenSlotSpansMultipleDays()
    {
        // Arrange
        var handler = new TakeSlotHandler(_dbContext);
        var request = new TakeSlotRequest(
            Start: new DateTime(2024, 9, 25, 22, 0, 0),
            End: new DateTime(2024, 9, 26, 2, 0, 0),
            Comments: "Invalid slot",
            Patient: new TakeSlotPatientModel("Jane", "Doe", "jane.doe@example.com", "987654321")
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidSlotException>(() => handler.Handle(request, CancellationToken.None));
    }

    private void SeedDatabase(SlotsAppDbContext dbContext)
    {
        var availability = TestData.SampleAvailability();
        dbContext.Availabilities.Add(availability);
        dbContext.SaveChanges();
    }
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
