using Microsoft.Extensions.DependencyInjection;
using Snapper;
using Swashbuckle.AspNetCore.Swagger;

namespace DocPlanner.SlotsApp.Tests
{
    public class OpenApiTest : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;

        public OpenApiTest(TestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// This test detects potentially unwanted changes in the OpenAPI schema.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ValidateOpenApiSchema()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var swaggerGen = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
            var swaggerDoc = swaggerGen.GetSwagger("v1");

            // Assert
            swaggerDoc.ShouldMatchSnapshot();
        }

    }
}