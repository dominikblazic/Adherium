namespace Adherium.Adherence.Core.Domain.Enums;

/// <summary>How a medication is used, which determines how (or whether) adherence is scored.</summary>
public enum MedicationType
{
    /// <summary>Preventer taken on a fixed schedule — adherence to that schedule is meaningful.</summary>
    Controller,

    /// <summary>Rescue medication taken as-needed (PRN) — schedule adherence does not apply.</summary>
    Reliever,
}
