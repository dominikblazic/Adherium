using Adherium.Adherence.Core.Domain;

namespace Adherium.Adherence.Core.Results;

/// <summary>Outcome of processing a single event in the batch.</summary>
public enum ProcessingStatus
{
    /// <summary>Attributed and counted (newly stored).</summary>
    Processed,

    /// <summary>Already ingested previously (same <see cref="EventKey"/>) — ignored, not double-counted.</summary>
    DuplicateIgnored,

    /// <summary>Could not be attributed to any prescription — see the reason.</summary>
    Unattributed,
}

/// <summary>Per-event report so callers can see exactly what happened to each event, and why.</summary>
public sealed record EventOutcome
{
    public required string DeviceSerial { get; init; }
    public required int DeviceLogId { get; init; }
    public required DateTimeOffset EventTimestampUtc { get; init; }
    public required ProcessingStatus Status { get; init; }

    /// <summary>Human-readable explanation, present for non-<see cref="ProcessingStatus.Processed"/> outcomes.</summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Recalculated daily adherence for one patient/prescription/day.
/// <see cref="AdherenceRate"/> is <c>null</c> for relievers, where schedule adherence does not apply.
/// </summary>
public sealed record DailyAdherence
{
    public required int PatientId { get; init; }
    public required int PrescriptionId { get; init; }
    public required DateOnly Date { get; init; }
    public required MedicationType MedicationType { get; init; }
    public required int DosesPrescribed { get; init; }
    public required int DosesTaken { get; init; }

    /// <summary>doses taken / doses prescribed, as a percentage. Not capped at 100% (over-use is meaningful).</summary>
    public decimal? AdherenceRate { get; init; }
}

/// <summary>Headline counts for the batch.</summary>
public sealed record BatchSummary
{
    public required int Received { get; init; }
    public required int Processed { get; init; }
    public required int Duplicates { get; init; }
    public required int Unattributed { get; init; }
}

/// <summary>The full response: recalculated summaries plus a per-event audit trail.</summary>
public sealed record RecalculationResult
{
    public required BatchSummary Summary { get; init; }
    public required IReadOnlyList<DailyAdherence> DailyAdherence { get; init; }
    public required IReadOnlyList<EventOutcome> Outcomes { get; init; }
}
