using System.Collections.Concurrent;
using Adherium.Adherence.Core.Contracts.Repositories;
using Adherium.Adherence.Core.Domain.Entities;

namespace Adherium.Adherence.Core.Repositories;

public sealed class DeviceAssignmenRepository : IDeviceAssignmentRepository
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
