namespace Adherium.Adherence.Core.Results;

/// <summary>The full response: recalculated summaries plus a per-event audit trail.</summary>
public sealed record RecalculationResult
{
    public required BatchSummary Summary { get; init; }
    public required IReadOnlyList<DailyAdherence> DailyAdherence { get; init; }
    public required IReadOnlyList<EventOutcome> Outcomes { get; init; }
}
