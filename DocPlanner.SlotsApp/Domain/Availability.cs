using NodaTime;

namespace DocPlanner.SlotsApp.Domain
{
    public record Availability
    {
        private List<Appointment> _appointments = new();
        public Guid Id { get; init; } = Guid.NewGuid();
        public required Facility Facility { get; init; }
        public int SlotDurationMinutes { get; init; }

        private readonly List<WorkPeriod> _workPeriods = default!;
        public List<WorkPeriod> WorkPeriods
        {
            get => _workPeriods;
            init
            {
                try
                {
                    _ = value.ToDictionary(x => x.DayOfWeek);
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("There must be at most one work period for each day of the week.");
                }

                _workPeriods = value;
            }
        }

        public IReadOnlyList<Appointment> Appointments
        {
            get => _appointments;
            set => _appointments = [.. value];
        }

        public void AddAppointment(Appointment appointment)
        {
            var workPeriods = _workPeriods.ToDictionary(x => x.DayOfWeek);

            MaybeThrowIfNoWorkPeriodForDay(appointment.Date.DayOfWeek, workPeriods);
            MaybeThrowIfSlotOutsideWorkPeriodHours(appointment, workPeriods[appointment.Date.DayOfWeek]);
            MaybeThrowIfSlotNotOfDefinedDuration(appointment);
            MaybeThrowIfAppointmentOverlaps(appointment);

            _appointments.Add(appointment);
        }

        private void MaybeThrowIfSlotNotOfDefinedDuration(OccupiedSlot occupiedSlot)
        {
            var slotDuration = Period.Between(occupiedSlot.Start, occupiedSlot.End).ToDuration().TotalMinutes;

            if (slotDuration != SlotDurationMinutes)
            {
                throw new InvalidSlotException($"Requested slot should have duration of {SlotDurationMinutes} minutes.");
            }
        }

        private void MaybeThrowIfNoWorkPeriodForDay(IsoDayOfWeek isoDayOfWeek, Dictionary<IsoDayOfWeek, WorkPeriod> workPeriods)
        {
            if (!workPeriods.ContainsKey(isoDayOfWeek))
            {
                throw new InvalidSlotException("OccupiedSlot must be within a work period.");
            }
        }

        private static void MaybeThrowIfSlotOutsideWorkPeriodHours(OccupiedSlot occupiedSlot, WorkPeriod workPeriod)
        {
            if (workPeriod.OffHours.Any(x => x.OverlapsWith(occupiedSlot)))
            {
                throw new InvalidSlotException("OccupiedSlot must be within the work period hours.");
            }
        }

        private void MaybeThrowIfSlotOverlapsWithLunchHour(OccupiedSlot occupiedSlot, WorkPeriod workPeriod)
        {
            if (workPeriod.LunchBreak.OverlapsWith(occupiedSlot))
            {
                throw new InvalidSlotException("OccupiedSlot must be within the work period hours.");
            }
        }

        private void MaybeThrowIfAppointmentOverlaps(Appointment appointment)
        {
            var existingSlotsOnSameDay = Appointments.Where(x => x.Date == appointment.Date);

            if (existingSlotsOnSameDay.Any(x => x.OverlapsWith(appointment)))
            {
                throw new InvalidSlotException("The new occupied slot overlaps with an existing slot.");
            }
        }
    }
}