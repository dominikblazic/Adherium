using Adherium.Adherence.Core.Contracts;
using Adherium.Adherence.Core.Domain;
using Adherium.Adherence.Core.Stores;

namespace Adherium.Adherence.Core.Services;

/// <summary>Why an event could not be attributed to a prescription.</summary>
public enum AttributionStatus
{
    Attributed,

    /// <summary>No assignment history exists for the serial at all.</summary>
    UnknownDevice,

    /// <summary>The device is known, but no assignment was active at the event time (a gap, or before/after).</summary>
    NoActiveAssignment,

    /// <summary>An assignment was found but its prescription is missing — a data integrity problem.</summary>
    MissingPrescription,
}

/// <summary>The result of resolving an event to a prescription.</summary>
public readonly record struct AttributionResult(AttributionStatus Status, Prescription? Prescription)
{
    public bool IsAttributed => Status == AttributionStatus.Attributed && Prescription is not null;

    public static AttributionResult Attributed(Prescription prescription) =>
        new(AttributionStatus.Attributed, prescription);

    public static AttributionResult Failed(AttributionStatus status) => new(status, null);
}

public sealed class AttributionService(
    IDeviceAssignmentStore assignments,
    IPrescriptionStore prescriptions) : IAttributionService
{
    public AttributionResult Resolve(string deviceSerial, DateTimeOffset eventTimestampUtc)
    {
        if (!assignments.DeviceExists(deviceSerial))
        {
            return AttributionResult.Failed(AttributionStatus.UnknownDevice);
        }

        // If windows ever overlap (a data error), prefer the one that started most recently.
        var active = assignments.GetBySerial(deviceSerial)
            .Where(a => a.Covers(eventTimestampUtc))
            .OrderByDescending(a => a.StartUtc)
            .FirstOrDefault();

        if (active is null)
        {
            return AttributionResult.Failed(AttributionStatus.NoActiveAssignment);
        }

        var prescription = prescriptions.GetById(active.PrescriptionId);
        return prescription is null
            ? AttributionResult.Failed(AttributionStatus.MissingPrescription)
            : AttributionResult.Attributed(prescription);
    }
}
