using Adherium.Adherence.Api.Contracts;
using Adherium.Adherence.Core.Contracts.Services;
using Adherium.Adherence.Core.Domain.Dtos.Results;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Adherium.Adherence.Api.Endpoints;

public static class SensorLogEndpoints
{
    public static IEndpointRouteBuilder MapSensorLogEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Versioning (the version set and the /api/v{version} prefix) is configured once in Program.cs;
        // here we only declare the route suffix and which version these endpoints serve.
        RouteGroupBuilder v1 = app
            .MapGroup("/sensor-logs")
            .MapToApiVersion(new ApiVersion(1, 0))
            .WithTags("Sensor Logs");

        v1.MapPost("/recalculate", Recalculate)
            .WithName("RecalculateAdherence")
            .WithSummary("Ingest a batch of sensor log events and recalculate adherence.")
            .WithDescription(
                "Attributes each event to the prescription that was active for the device at the event " +
                "time, idempotently stamps it, and returns recalculated daily adherence plus a per-event " +
                "outcome report.")
            .Produces<RecalculationResult>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        return app;
    }

    private static Ok<RecalculationResult> Recalculate(
        RecalculateRequest request,
        IRecalculationService recalculationService)
    {
        var events = request.Events.Select(e => e.ToDomain()).ToList();
        return TypedResults.Ok(recalculationService.Recalculate(events));
    }
}
