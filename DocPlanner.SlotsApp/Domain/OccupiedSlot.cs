using NodaTime;

namespace DocPlanner.SlotsApp.Domain
{

    public record OccupiedSlot
    {
        public OccupiedSlot(LocalTime start, LocalTime end)
        {
            if (start >= end)
            {
                throw new ArgumentException("The start date must be earlier than the end date.");
            }

            Start = start;
            End = end;
        }

        public LocalTime Start { get; init; }
        public LocalTime End { get; init; }

        public bool OverlapsWith(OccupiedSlot other)
        {
            return Start < other.End && End > other.Start;
        }
    }
}

