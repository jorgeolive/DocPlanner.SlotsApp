using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace DocPlanner.SlotsApp.Tests.Api;

public class BasicAuthTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;

    public BasicAuthTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TakeSlot_Endpoint_RequiresAuthentication()
    {
        // Arrange
        var client = _factory.CreateClient();

        var takeSlotRequest = new
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddMinutes(30),
            Comments = "Test appointment",
            Patient = new
            {
                Name = "John",
                SecondName = "Doe",
                Email = "john.doe@example.com",
                Phone = "1234567890"
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/availability/takeSlot", takeSlotRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TakeSlot_Endpoint_FailsWithNotValidAuth()
    {
        // Arrange
        var client = _factory.CreateClient();

        var credentials = Encoding.UTF8.GetBytes("mamerto:wrongpassword");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(credentials));

        var takeSlotRequest = new
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddMinutes(30),
            Comments = "Test appointment",
            Patient = new
            {
                Name = "John",
                SecondName = "Doe",
                Email = "r3r"
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/availability/takeSlot", takeSlotRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TakeSlot_Endpoint_AllowsAuthenticatedUsers()
    {
        // Arrange
        var client = _factory.CreateClient();


        var credentials = Encoding.UTF8.GetBytes("mamerto:mellon");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(credentials));

        var takeSlotRequest = new
        {
            Start = new DateTime(2024, 09, 23, 11, 0, 0),
            End = new DateTime(2024, 09, 23, 11, 0, 0).AddMinutes(30),
            Comments = "Test appointment",
            Patient = new
            {
                Name = "John",
                SecondName = "Doe",
                Email = "john.doe@example.com",
                Phone = "1234567890"
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/availability/takeSlot", takeSlotRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}