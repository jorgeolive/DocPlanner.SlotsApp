using NodaTime;

namespace DocPlanner.SlotsApp.Domain
{
    public record Appointment : OccupiedSlot
    {
        public Guid AvailabilityId { get; init; }
        public Guid Id { get; init; } = Guid.NewGuid();
        public NodaTime.LocalDate Date { get; init; }
        public Facility Facility { get; } = default!;
        public string Comments { get; init; }
        public required Patient Patient { get; init; }
        public Appointment(LocalDate date, LocalTime start, LocalTime end, string comments) : base(start, end)
        {
            Date = date;
            Comments = comments;
        }
    }
}

