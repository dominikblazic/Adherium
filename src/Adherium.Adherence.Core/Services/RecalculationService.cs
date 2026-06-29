using Adherium.Adherence.Core.Contracts.Repositories;
using Adherium.Adherence.Core.Contracts.Services;
using Adherium.Adherence.Core.Domain.Entities;
using Adherium.Adherence.Core.Domain.Models;
using Adherium.Adherence.Core.Extensions;
using Adherium.Adherence.Core.Results;
using Adherium.Adherence.Core.Results.Enums;

namespace Adherium.Adherence.Core.Services;

public sealed class RecalculationService(
    IAttributionService attributionService,
    IStampedLogRepository stampedLogRepository,
    IAdherenceCalculator adherenceCalculator) : IRecalculationService
{
    public RecalculationResult Recalculate(IReadOnlyCollection<LogEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var outcomes = new List<EventOutcome>(events.Count);
        var affectedPrescriptions = new HashSet<int>();
        var affectedDays = new HashSet<(int PrescriptionId, DateOnly Date)>();

        foreach (var @event in events)
        {
            var result = attributionService.Resolve(@event.DeviceSerial, @event.EventTimestampUtc);

            if (!result.IsAttributed)
            {
                outcomes.Add(Unattributed(@event, result.Status));
                continue;
            }

            var prescription = result.Prescription!;
            var stamped = new StampedLog
            {
                DeviceSerial = @event.DeviceSerial,
                DeviceLogId = @event.DeviceLogId,
                EventTimestampUtc = @event.EventTimestampUtc,
                EventType = @event.EventType.ToEventType(),
                PrescriptionId = prescription.Id,
                PatientId = prescription.PatientId,
            };

            var added = stampedLogRepository.TryAdd(stamped);
            outcomes.Add(added ? Processed(@event) : Duplicate(@event));

            // A day is "affected" whenever this batch touches it — newly stored OR a re-sent
            // duplicate — so re-sending the same batch returns identical (idempotent) summaries.
            affectedPrescriptions.Add(prescription.Id);
            affectedDays.Add((prescription.Id, stamped.UtcDate));
        }

        // Recompute from the full stored set (not just this batch) so the summaries are always the
        // authoritative recalculated state for the affected days.
        var logsToScore = stampedLogRepository
            .GetForPrescriptions(affectedPrescriptions)
            .Where(log => affectedDays.Contains((log.PrescriptionId, log.UtcDate)));

        var daily = adherenceCalculator.Calculate(logsToScore);

        return new RecalculationResult
        {
            Summary = Summarize(outcomes),
            DailyAdherence = daily,
            Outcomes = outcomes,
        };
    }

    private static EventOutcome Processed(LogEvent e) => Outcome(e, ProcessingStatus.Processed, null);

    private static EventOutcome Duplicate(LogEvent e) =>
        Outcome(e, ProcessingStatus.DuplicateIgnored, "Already ingested (duplicate deviceLogId for this serial).");

    private static EventOutcome Unattributed(LogEvent e, AttributionStatus status) =>
        Outcome(e, ProcessingStatus.Unattributed, ReasonFor(status));

    private static EventOutcome Outcome(LogEvent e, ProcessingStatus status, string? reason) => new()
    {
        DeviceSerial = e.DeviceSerial,
        DeviceLogId = e.DeviceLogId,
        EventTimestampUtc = e.EventTimestampUtc,
        Status = status,
        Reason = reason,
    };

    private static string ReasonFor(AttributionStatus status) => status switch
    {
        AttributionStatus.UnknownDevice => "No assignment history exists for this device serial.",
        AttributionStatus.NoActiveAssignment => "No prescription was assigned to this device at the event time (gap or reassignment).",
        AttributionStatus.MissingPrescription => "The active assignment references a prescription that does not exist.",
        _ => "Could not be attributed.",
    };

    private static BatchSummary Summarize(List<EventOutcome> outcomes) => new()
    {
        Received = outcomes.Count,
        Processed = outcomes.Count(o => o.Status == ProcessingStatus.Processed),
        Duplicates = outcomes.Count(o => o.Status == ProcessingStatus.DuplicateIgnored),
        Unattributed = outcomes.Count(o => o.Status == ProcessingStatus.Unattributed),
    };
}
