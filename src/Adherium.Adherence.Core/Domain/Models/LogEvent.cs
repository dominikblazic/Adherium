namespace Adherium.Adherence.Core.Domain.Models;

/// <summary>
/// An incoming sensor log event from a device batch. <see cref="EventType"/> is kept as the raw
/// string from the wire and parsed via event type so that an
/// unknown value degrades gracefully instead of failing the whole batch.
/// </summary>
public sealed record LogEvent
{
    public required string DeviceSerial { get; init; }

    /// <summary>The device's own sequence number for the event.</summary>
    public required int DeviceLogId { get; init; }

    public required DateTimeOffset EventTimestampUtc { get; init; }

    public required string EventType { get; init; }
}
