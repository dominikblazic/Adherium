namespace Adherium.Adherence.Core.Domain.Entities;

/// <summary>
/// The temporal record of which prescription a device served over a time window.
/// This is what lets us attribute a historical event to the prescription that was
/// active <em>at the time of the event</em>, rather than the device's current owner.
/// </summary>
public sealed record DeviceAssignment
{
    public required int Id { get; init; }
    public required string DeviceSerial { get; init; }
    public required int PrescriptionId { get; init; }
    public required DateTimeOffset StartUtc { get; init; }
    public DateTimeOffset? EndUtc { get; init; }

    /// <summary>
    /// Whether this assignment was active at <paramref name="timestamp"/>.
    /// The window is treated as <c>[StartUtc, EndUtc]</c> — <b>end-inclusive</b> — to match the
    /// sample data, which uses end-of-day (<c>23:59:59</c>) ends with explicit gaps between windows.
    /// </summary>
    public bool Covers(DateTimeOffset timestamp) =>
        timestamp >= StartUtc && (EndUtc is null || timestamp <= EndUtc.Value);
}
