using DocPlanner.SlotsApp.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace DocPlanner.SlotsApp.Features.Availability.Get;

public record FacilityModel(string Name, string Address);
public record WeeklyAvailabilityModel(
    FacilityModel Facility,
    int SlotDurationInMinutes,
    WorkDayModel Monday,
    WorkDayModel Tuesday,
    WorkDayModel Wednesday,
    WorkDayModel Thursday,
    WorkDayModel Friday,
    WorkDayModel Saturday,
    WorkDayModel Sunday);

public record WorkDayModel(WorkPeriodModel Workperiod);
public record WorkPeriodModel(int StartHour, int LunchStartHour, int LunchEndHour, int EndHour, IEnumerable<BusySlotModel> BusySlot);
public record BusySlotModel(DateTime Start, DateTime End);

public static class GetWeeklyAvailabilityApi
{
    public static async Task<IResult> Endpoint([FromRoute]string yyyyMMdd, IMediator mediator, CancellationToken ct)
    {
        try
        {
            var dateParsed = new GetWeeklyAvailabilityRequest(DateOnly.ParseExact(yyyyMMdd, "yyyyMMdd"));
        } 
        catch (FormatException)
        {
            return Results.BadRequest("Invalid date format.");
        }

        try
        {
            return Results.Ok(
                await mediator.Send(new GetWeeklyAvailabilityRequest(DateOnly.ParseExact(yyyyMMdd, "yyyyMMdd")), ct));
        }
        catch (InvalidGetWeeklyAvailabilityDateException)
        {
            return Results.BadRequest("Invalid date: Not a monday.");
        }
    }
}

public record GetWeeklyAvailabilityRequest(DateOnly Date) : IRequest<WeeklyAvailabilityModel>;

public record GetWeeklyAvailabilityHander(SlotsAppDbContext dbContext) : IRequestHandler<GetWeeklyAvailabilityRequest, WeeklyAvailabilityModel>
{
    public async Task<WeeklyAvailabilityModel> Handle(GetWeeklyAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var requestDate = new LocalDate(request.Date.Year, request.Date.Month, request.Date.Day);

        MaybeThrowIfNotMonday(requestDate);

        Domain.Availability facility = await GetFacilityWithRequestedWeekAppointments(requestDate, cancellationToken);

        return AvailabilityMapper.MapToWeeklyAvailabilityModel(facility);
    }

    private async Task<Domain.Availability> GetFacilityWithRequestedWeekAppointments(LocalDate monday, CancellationToken cancellationToken)
    {
        return await dbContext.Availabilities
                    .Include(x => x.Appointments
                        .Where(x => x.Date >= monday && x.Date <= monday.PlusDays(6)))
                .SingleAsync(cancellationToken);
    }

    private static void MaybeThrowIfNotMonday(LocalDate localDate)
    {
        if (localDate!.DayOfWeek != IsoDayOfWeek.Monday)
        {
            throw new InvalidGetWeeklyAvailabilityDateException();
        }
    }
}
