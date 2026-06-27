using Adherium.Adherence.Api.Contracts;
using Adherium.Adherence.Core.Contracts;
using Adherium.Adherence.Core.Results;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Adherium.Adherence.Api.Endpoints;

public static class SensorLogEndpoints
{
    public static IEndpointRouteBuilder MapSensorLogEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        ApiVersionSet versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder v1 = app
            .MapGroup("/api/v{version:apiVersion}/sensor-logs")
            .WithApiVersionSet(versionSet)
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
