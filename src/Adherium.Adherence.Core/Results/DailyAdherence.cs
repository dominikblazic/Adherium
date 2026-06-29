using Adherium.Adherence.Core.Domain.Enums;

namespace Adherium.Adherence.Core.Results;

public sealed record DailyAdherence
{
    public required int PatientId { get; init; }
    public required int PrescriptionId { get; init; }
    public required DateOnly Date { get; init; }
    public required MedicationType MedicationType { get; init; }
    public required int DosesPrescribed { get; init; }
    public required int DosesTaken { get; init; }
    public decimal? AdherenceRate { get; init; }
}
