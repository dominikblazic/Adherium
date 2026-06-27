namespace Adherium.Adherence.Core.Domain;

/// <summary>Kinds of sensor log events. Only <see cref="Actuation"/> counts as a taken dose.</summary>
public enum EventType
{
    /// <summary>Unrecognised event type — stamped for traceability but never counted as a dose.</summary>
    Unknown = 0,

    /// <summary>A dose (puff) was taken.</summary>
    Actuation,

    /// <summary>A peak inhalation flow measurement — diagnostic, not a dose.</summary>
    PeakInhalationFlow,
}

/// <summary>
/// An incoming sensor log event from a device batch. <see cref="EventType"/> is kept as the raw
/// string from the wire and parsed via <see cref="EventTypeExtensions.ToEventType"/> so that an
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

public static class EventTypeExtensions
{
    /// <summary>Parses a raw event-type string, falling back to <see cref="EventType.Unknown"/>.</summary>
    public static EventType ToEventType(this string? raw) =>
        Enum.TryParse<EventType>(raw, ignoreCase: true, out var parsed) ? parsed : EventType.Unknown;
}
