using System.Collections.Concurrent;
using Adherium.Adherence.Core.Domain;

namespace Adherium.Adherence.Core.Stores;

/// <summary>Read access to the seeded device-assignment history.</summary>
public interface IDeviceAssignmentStore
{
    /// <summary>All assignments ever recorded for a serial (empty if the device is unknown).</summary>
    IReadOnlyCollection<DeviceAssignment> GetBySerial(string deviceSerial);

    bool DeviceExists(string deviceSerial);

    void Add(DeviceAssignment assignment);
}

/// <summary>In-memory device-assignment store, indexed by serial.</summary>
public sealed class InMemoryDeviceAssignmentStore : IDeviceAssignmentStore
{
    private readonly ConcurrentDictionary<string, List<DeviceAssignment>> _bySerial =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<DeviceAssignment> GetBySerial(string deviceSerial) =>
        _bySerial.TryGetValue(deviceSerial, out var list) ? list : [];

    public bool DeviceExists(string deviceSerial) => _bySerial.ContainsKey(deviceSerial);

    public void Add(DeviceAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        _bySerial.AddOrUpdate(
            assignment.DeviceSerial,
            _ => [assignment],
            (_, existing) =>
            {
                existing.Add(assignment);
                return existing;
            });
    }
}
