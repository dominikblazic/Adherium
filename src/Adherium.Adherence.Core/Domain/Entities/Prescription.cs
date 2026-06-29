using Adherium.Adherence.Core.Domain.Enums;

namespace Adherium.Adherence.Core.Domain.Entities;

public sealed record Prescription
{
    public required int Id { get; init; }
    public required int PatientId { get; init; }
    public required MedicationType MedicationType { get; init; }

    public required int DosesPerAdmin { get; init; }

    public required int TimesPerDay { get; init; }

    public required DateTimeOffset StartUtc { get; init; }

    public DateTimeOffset? EndUtc { get; init; }

    public int DailyPrescribedDoses => DosesPerAdmin * TimesPerDay;

    public bool CountsTowardAdherence => MedicationType == MedicationType.Controller;
}
