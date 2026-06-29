using Adherium.Adherence.Core.Results.Enums;

namespace Adherium.Adherence.Core.Results;

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
