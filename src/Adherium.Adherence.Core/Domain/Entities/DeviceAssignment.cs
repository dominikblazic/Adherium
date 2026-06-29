namespace Adherium.Adherence.Core.Domain.Entities;

public sealed record DeviceAssignment
{
    public required int Id { get; init; }
    public required string DeviceSerial { get; init; }
    public required int PrescriptionId { get; init; }
    public required DateTimeOffset StartUtc { get; init; }
    public DateTimeOffset? EndUtc { get; init; }

    public bool Covers(DateTimeOffset timestamp) =>
        timestamp >= StartUtc && (EndUtc is null || timestamp <= EndUtc.Value);
}
