using Adherium.Adherence.Core.Domain.Enums;

namespace Adherium.Adherence.Core.Domain.Entities;

/// <summary>
/// A prescription for a patient. The scheduled daily dose count is <c>DosesPerAdmin × TimesPerDay</c>.
/// </summary>
public sealed record Prescription
{
    public required int Id { get; init; }
    public required int PatientId { get; init; }
    public required MedicationType MedicationType { get; init; }

    /// <summary>Puffs per scheduled administration.</summary>
    public required int DosesPerAdmin { get; init; }

    /// <summary>Scheduled administrations per day.</summary>
    public required int TimesPerDay { get; init; }

    public required DateTimeOffset StartUtc { get; init; }

    /// <summary><c>null</c> means open-ended.</summary>
    public DateTimeOffset? EndUtc { get; init; }

    /// <summary>Scheduled doses for a full day under this prescription.</summary>
    public int DailyPrescribedDoses => DosesPerAdmin * TimesPerDay;

    /// <summary>
    /// Whether a schedule-adherence rate should be reported. Relievers are taken as-needed,
    /// so a "percent of scheduled doses taken" is clinically meaningless for them.
    /// </summary>
    public bool CountsTowardAdherence => MedicationType == MedicationType.Controller;
}
