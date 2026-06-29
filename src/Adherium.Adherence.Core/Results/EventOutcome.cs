using Adherium.Adherence.Core.Results.Enums;

namespace Adherium.Adherence.Core.Results;

public sealed record EventOutcome
{
    public required string DeviceSerial { get; init; }
    public required int DeviceLogId { get; init; }
    public required DateTimeOffset EventTimestampUtc { get; init; }
    public required ProcessingStatus Status { get; init; }

    public string? Reason { get; init; }
}
