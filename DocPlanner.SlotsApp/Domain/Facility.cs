namespace DocPlanner.SlotsApp.Domain;

public record Facility
{
    public required string Name { get; init; }
    public required string Address { get; init; }
}
