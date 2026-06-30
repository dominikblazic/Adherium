using Adherium.Adherence.Core.Contracts.Repositories;
using Adherium.Adherence.Core.Contracts.Services;
using Adherium.Adherence.Core.Domain.Entities;
using Adherium.Adherence.Core.Domain.Enums;
using Adherium.Adherence.Core.Results;

namespace Adherium.Adherence.Core.Services;

public sealed class AdherenceCalculator(IPrescriptionRepository prescriptionRepository) : IAdherenceCalculator
{
    public IReadOnlyList<DailyAdherence> Calculate(IEnumerable<StampedLog> stampedLogs)
    {
        ArgumentNullException.ThrowIfNull(stampedLogs);

        var summaries = new List<DailyAdherence>();

        var groups = stampedLogs.GroupBy(log => (log.PrescriptionId, log.UtcDate));

        foreach (var group in groups)
        {
            var prescription = prescriptionRepository.GetById(group.Key.PrescriptionId);
            if (prescription is null)
            {
                continue; 
            }

            var dosesTaken = group.Count(log => log.EventType == EventType.Actuation);
            var dosesPrescribed = prescription.DailyPrescribedDoses;

            summaries.Add(new DailyAdherence
            {
                PatientId = prescription.PatientId,
                PrescriptionId = prescription.Id,
                Date = group.Key.UtcDate,
                MedicationType = prescription.MedicationType,
                DosesPrescribed = dosesPrescribed,
                DosesTaken = dosesTaken,
                AdherenceRate = ComputeRate(prescription, dosesTaken, dosesPrescribed),
            });
        }

        return [.. summaries
            .OrderBy(s => s.PatientId)
            .ThenBy(s => s.PrescriptionId)
            .ThenBy(s => s.Date)];
    }

    private static decimal? ComputeRate(Prescription prescription, int dosesTaken, int dosesPrescribed)
    {
        if (!prescription.CountsTowardAdherence || dosesPrescribed <= 0)
        {
            return null;
        }

        return Math.Round((decimal)dosesTaken / dosesPrescribed * 100m, 2);
    }
}
