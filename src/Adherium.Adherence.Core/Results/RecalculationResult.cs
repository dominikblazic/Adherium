namespace Adherium.Adherence.Core.Results;

public sealed record RecalculationResult
{
    public required BatchSummary Summary { get; init; }
    public required IReadOnlyList<DailyAdherence> DailyAdherence { get; init; }
    public required IReadOnlyList<EventOutcome> Outcomes { get; init; }
}
