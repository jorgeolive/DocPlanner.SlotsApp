using DocPlanner.SlotsApp.Persistence;
using DocPlanner.SlotsApp.Tests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly string _inMemoryDatabaseName = Guid.NewGuid().ToString();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Add(new JsonConfigurationSource
            {
                Path = "appsettings.integration.json",
                Optional = false,
                ReloadOnChange = true,
                FileProvider = new PhysicalFileProvider(Environment.CurrentDirectory + "/Api")
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SlotsAppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<SlotsAppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_inMemoryDatabaseName);
            });

            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<SlotsAppDbContext>();

                db.Database.EnsureCreated();

                SeedDatabase(db);
            }
        });
    }

    void SeedDatabase(SlotsAppDbContext context)
    {
        context.Availabilities.Add(TestData.SampleAvailability());
        context.SaveChanges();
    }
}