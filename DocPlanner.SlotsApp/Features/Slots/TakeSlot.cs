using DocPlanner.SlotsApp.Domain;
using DocPlanner.SlotsApp.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace DocPlanner.SlotsApp.Features.Slots
{
    public class TakeSlotApi
    {
        public static async Task<IResult> Endpoint(TakeSlotRequest takeSlotRequest, CancellationToken ct, IMediator mediator)
        {
            await mediator.Send(takeSlotRequest, cancellationToken: ct);

            return Results.Created();
        }
    }
    public record TakeSlotRequest(DateTime Start, DateTime End, string Comments, TakeSlotPatientModel Patient) : IRequest;

    public record TakeSlotPatientModel(string Name, string SecondName, string Email, string Phone);

    public class TakeSlotHandler(SlotsAppDbContext dbContext) : IRequestHandler<TakeSlotRequest>
    {
        public async Task Handle(TakeSlotRequest command, CancellationToken cancellationToken)
        {
            if (command.Start.Date != command.End.Date)
            {
                throw new InvalidSlotException("Slot cannot span multiple days.");
            }

            var availability = await dbContext.Availabilities
                .Include(x => x.Appointments.Where(x => x.Date == LocalDate.FromDateTime(command.Start.Date)))
                .SingleAsync(cancellationToken);

            availability.AddAppointment(
                new Appointment(
                    LocalDate.FromDateTime(command.Start.Date),
                    new LocalTime(command.Start.Hour, command.Start.Minute),
                    new LocalTime(command.End.Hour, command.End.Minute),
                    command.Comments)
                {
                    Patient = new Patient(command.Patient.Name, command.Patient.SecondName, command.Patient.Email, command.Patient.Phone)
                });

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

