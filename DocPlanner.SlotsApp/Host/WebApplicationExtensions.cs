using DocPlanner.SlotsApp.Features.Availability.Get;
using DocPlanner.SlotsApp.Features.Slots;
using Microsoft.OpenApi.Models;

namespace DocPlanner.SlotsApp.Host;

public static class WebApplicationExtensions
{
    public static WebApplication RegisterSlotAppEndpoints(this WebApplication app)
    {
        //TODO use map groups
        app.MapGet("/api/availability/GetWeeklyAvailability/{yyyyMMdd}", GetWeeklyAvailabilityApi.Endpoint)
        .WithName("GetWeeklyAvailability")
        .WithOpenApi(generatedOperation =>
        {

            generatedOperation.Security.Add(new OpenApiSecurityRequirement()
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "basic" } }] = new List<string>()
            });

            var parameter = generatedOperation.Parameters[0];
            parameter.Description = "A date in {yyyyMMdd} format. It should be a monday.";

            generatedOperation.Description = "Get the weekly availability for a given week.";
            generatedOperation.Tags = [new OpenApiTag { Name = "Availability", Description = "Operations related to availability." }];

            return generatedOperation;
        }).RequireAuthorization();


        app.MapPost("/api/availability/takeSlot", TakeSlotApi.Endpoint)
        .WithName("TakeSlot")
        .RequireAuthorization()
        .WithOpenApi(generatedOperation =>
        {
            generatedOperation.Description = "Get the weekly availability for a given week.";
            generatedOperation.Tags = [new OpenApiTag { Name = "Slots", Description = "Operations related to Appointments." }];

            generatedOperation.Responses = new OpenApiResponses
            {
                ["201"] = new OpenApiResponse
                {
                    Description = "Created",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["message"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "The slot was successfully created."
                                    }
                                }
                            }
                        }
                    }
                },
                ["422"] = new OpenApiResponse
                {
                    Description = "Unprocessable Entity",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["message"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "The slot could not be created."
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return generatedOperation;
        });

        return app;
    }
}