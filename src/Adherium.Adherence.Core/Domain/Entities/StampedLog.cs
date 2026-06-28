using Adherium.Adherence.Core.Domain.Enums;

namespace Adherium.Adherence.Core.Domain.Entities;

/// <summary>
/// Idempotency key for an ingested event: the device's serial plus its own sequence number.
/// See the README for the known caveat about sequence resets after refurbishment.
/// </summary>
public readonly record struct EventKey(string DeviceSerial, int DeviceLogId);

/// <summary>
/// An event after attribution: the prescription/patient that was active at the event time has been
/// resolved and stamped on permanently. Every attributed event is stored for traceability, even
/// non-actuation types; only actuations later count toward doses taken.
/// </summary>
public sealed record StampedLog
{
    public required string DeviceSerial { get; init; }
    public required int DeviceLogId { get; init; }
    public required DateTimeOffset EventTimestampUtc { get; init; }
    public required EventType EventType { get; init; }
    public required int PrescriptionId { get; init; }
    public required int PatientId { get; init; }

    public EventKey Key => new(DeviceSerial, DeviceLogId);

    /// <summary>The UTC calendar day this event is counted against.</summary>
    public DateOnly UtcDate => DateOnly.FromDateTime(EventTimestampUtc.UtcDateTime);
}
