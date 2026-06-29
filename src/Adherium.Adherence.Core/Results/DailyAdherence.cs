using Adherium.Adherence.Core.Domain.Enums;

namespace Adherium.Adherence.Core.Results;

/// <summary>
/// Recalculated daily adherence for one patient/prescription/day.
/// <see cref="AdherenceRate"/> is <c>null</c> for relievers, where schedule adherence does not apply.
/// </summary>
public sealed record DailyAdherence
{
    public required int PatientId { get; init; }
    public required int PrescriptionId { get; init; }
    public required DateOnly Date { get; init; }
    public required MedicationType MedicationType { get; init; }
    public required int DosesPrescribed { get; init; }
    public required int DosesTaken { get; init; }

    /// <summary>doses taken / doses prescribed, as a percentage. Not capped at 100% (over-use is meaningful).</summary>
    public decimal? AdherenceRate { get; init; }
}
