using NodaTime;

namespace DocPlanner.SlotsApp.Domain
{
    public record OffHours : OccupiedSlot
    {
        public OffHours(LocalTime start, LocalTime end) : base(start, end)
        {
        }
    }
}

