using Adherium.Adherence.Core.Domain.Entities;

namespace Adherium.Adherence.Core.Contracts.Repositories;

public interface IDeviceAssignmentRepository
{
    IReadOnlyCollection<DeviceAssignment> GetBySerial(string deviceSerial);
    bool DeviceExists(string deviceSerial);
    void Add(DeviceAssignment assignment);
}
