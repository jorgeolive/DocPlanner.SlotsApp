using DocPlanner.SlotsApp.Features.Availability.Get;
using FluentAssertions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace DocPlanner.SlotsApp.Tests.Api;

public class AvailabilityTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _authenticatedClient;

    public AvailabilityTests(TestWebApplicationFactory<Program> factory)
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
    public async Task CanGetWeeklyAvailability()
    {
        // Act
        var response = await _authenticatedClient.GetFromJsonAsync<WeeklyAvailabilityModel>("/api/availability/GetWeeklyAvailability/20240923");

        // Assert
        response!.Facility.Name.Should().Be("Main Clinic");
        response.Facility.Address.Should().Be("My Clinic");
        response.SlotDurationInMinutes.Should().Be(30);
        response.Thursday.Workperiod.StartHour.Should().Be(10);
        response.Thursday.Workperiod.EndHour.Should().Be(18);
        response.Thursday.Workperiod.LunchStartHour.Should().Be(13);
        response.Thursday.Workperiod.LunchEndHour.Should().Be(14);

        response.Wednesday.Workperiod.BusySlot.Count().Should().Be(1);
    }

    [Theory]
    [InlineData("202423222")]
    [InlineData("2024-12-2")]
    [InlineData("20241341")]
    public async Task CantPassIncorrectDateNumberAsRouteParameter(string wrongDate)
    {
        // Act
        var response = await _authenticatedClient.GetAsync($"/api/availability/GetWeeklyAvailability/{wrongDate}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
