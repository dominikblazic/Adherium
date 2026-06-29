using Adherium.Adherence.Core.Contracts.Repositories;
using Adherium.Adherence.Core.Contracts.Services;
using Adherium.Adherence.Core.Results;
using Adherium.Adherence.Core.Results.Enums;

namespace Adherium.Adherence.Core.Services;

public sealed class AttributionService(
    IDeviceAssignmentRepository deviceAssignmentRepository,
    IPrescriptionRepository prescriptionRepository) : IAttributionService
{
    public AttributionResult Resolve(string deviceSerial, DateTimeOffset eventTimestampUtc)
    {
        if (!deviceAssignmentRepository.DeviceExists(deviceSerial))
        {
            return AttributionResult.Failed(AttributionStatus.UnknownDevice);
        }

        var active = deviceAssignmentRepository.GetBySerial(deviceSerial)
            .Where(a => a.Covers(eventTimestampUtc))
            .OrderByDescending(a => a.StartUtc)
            .FirstOrDefault();

        if (active is null)
        {
            return AttributionResult.Failed(AttributionStatus.NoActiveAssignment);
        }

        var prescription = prescriptionRepository.GetById(active.PrescriptionId);
        return prescription is null
            ? AttributionResult.Failed(AttributionStatus.MissingPrescription)
            : AttributionResult.Attributed(prescription);
    }
}
