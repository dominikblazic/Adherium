namespace Adherium.Adherence.Core.Domain.Enums;

/// <summary>Kinds of sensor log events. Only <see cref="Actuation"/> counts as a taken dose.</summary>
public enum EventType
{
    /// <summary>Unrecognised event type — stamped for traceability but never counted as a dose.</summary>
    Unknown = 0,

    /// <summary>A dose (puff) was taken.</summary>
    Actuation,

    /// <summary>A peak inhalation flow measurement — diagnostic, not a dose.</summary>
    PeakInhalationFlow,
}
