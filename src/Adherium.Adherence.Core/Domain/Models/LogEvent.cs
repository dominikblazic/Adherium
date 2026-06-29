namespace Adherium.Adherence.Core.Domain.Models;

public sealed record LogEvent
{
    public required string DeviceSerial { get; init; }

    public required int DeviceLogId { get; init; }

    public required DateTimeOffset EventTimestampUtc { get; init; }

    public required string EventType { get; init; }
}
