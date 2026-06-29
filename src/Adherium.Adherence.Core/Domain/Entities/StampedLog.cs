using Adherium.Adherence.Core.Domain.Enums;

namespace Adherium.Adherence.Core.Domain.Entities;

public readonly record struct EventKey(string DeviceSerial, int DeviceLogId);

public sealed record StampedLog
{
    public required string DeviceSerial { get; init; }
    public required int DeviceLogId { get; init; }
    public required DateTimeOffset EventTimestampUtc { get; init; }
    public required EventType EventType { get; init; }
    public required int PrescriptionId { get; init; }
    public required int PatientId { get; init; }

    public EventKey Key => new(DeviceSerial, DeviceLogId);

    public DateOnly UtcDate => DateOnly.FromDateTime(EventTimestampUtc.UtcDateTime);
}
