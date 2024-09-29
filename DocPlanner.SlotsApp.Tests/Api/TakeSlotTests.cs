using DocPlanner.SlotsApp.Features.Availability.Get;
using DocPlanner.SlotsApp.Features.Slots;
using DocPlanner.SlotsApp.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace DocPlanner.SlotsApp.Tests.Api;

public class TakeSlotTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _authenticatedClient;

    public TakeSlotTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        var client = _factory.CreateClient();

        var credentials = Encoding.UTF8.GetBytes("mamerto:mellon");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(credentials));

        _authenticatedClient = client;
    }

    [Fact]
    public async Task CanAddSlot_HappyPath()
    {
        // Act
        var response = await _authenticatedClient.PostAsJsonAsync<TakeSlotRequest>(
            "/api/availability/takeSlot",
            new TakeSlotRequest(
                new DateTime(2024, 09, 26, 16, 00, 00),
                new DateTime(2024, 09, 26, 16, 30, 00),
                "blah",
                new TakeSlotPatientModel("aName", "aSurname", "email@c.com", "2344")));

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var updatedAvailability = await _authenticatedClient.GetFromJsonAsync<WeeklyAvailabilityModel>("/api/availability/GetWeeklyAvailability/20240923");

        updatedAvailability!.Thursday.Workperiod.BusySlot.Single().Start.Should().Be(new DateTime(2024, 09, 26, 16, 0, 0));
        updatedAvailability.Thursday.Workperiod.BusySlot.Single().End.Should().Be(new DateTime(2024, 09, 26, 16, 30, 0));

        var dbContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<SlotsAppDbContext>();

        var createdApp = dbContext.Appointments.Single(x => x.Start == new NodaTime.LocalTime(16, 00) && x.End == new NodaTime.LocalTime(16, 30) && x.Date == new NodaTime.LocalDate(2024,09,26));

        createdApp.Patient.Name.Should().Be("aName");
        createdApp.Patient.SecondName.Should().Be("aSurname");
        createdApp.Patient.Email.Should().Be("email@c.com");
        createdApp.Patient.Phone.Should().Be("2344");
    }
}