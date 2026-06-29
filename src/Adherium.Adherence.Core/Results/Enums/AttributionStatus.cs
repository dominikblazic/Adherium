namespace Adherium.Adherence.Core.Results.Enums;

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
