using Adherium.Adherence.Core.Domain.Enums;
using Adherium.Adherence.Core.Repositories;
using Adherium.Adherence.Core.Results.Enums;
using Adherium.Adherence.Core.Services;
using static Adherium.Adherence.Core.Tests.TestData;

namespace Adherium.Adherence.Core.Tests;

public sealed class RecalculationServiceTests
{
    private static RecalculationService BuildService()
    {
        var assignments = new DeviceAssignmenRepository();
        assignments.Add(Assignment(1, "DEV-AAA", 100, "2026-03-01T00:00:00Z", "2026-03-04T23:59:59Z"));
        assignments.Add(Assignment(2, "DEV-AAA", 200, "2026-03-05T00:00:00Z", "2026-03-07T23:59:59Z"));
        assignments.Add(Assignment(3, "DEV-AAA", 300, "2026-03-10T00:00:00Z"));

        var prescriptions = new PrescriptionRepository();
        prescriptions.Add(Prescription(100, dosesPerAdmin: 2, timesPerDay: 2, patientId: 1));
        prescriptions.Add(Prescription(200, dosesPerAdmin: 1, timesPerDay: 2, patientId: 1));
        prescriptions.Add(Prescription(300, type: MedicationType.Reliever, patientId: 2));

        var stampedLogs = new StampedLogRepository();
        var calculator = new AdherenceCalculator(prescriptions);
        var attribution = new AttributionService(assignments, prescriptions);
        return new RecalculationService(attribution, stampedLogs, calculator);
    }

    [Fact]
    public void Batch_summary_counts_processed_duplicate_and_unattributed()
    {
        var service = BuildService();

        var result = service.Recalculate(
        [
            Event("DEV-AAA", 11, "2026-03-02T08:00:00Z"),                       
            Event("DEV-AAA", 11, "2026-03-02T08:00:00Z"),                       
            Event("DEV-AAA", 40, "2026-03-08T10:00:00Z"),                       
            Event("DEV-ZZZ", 1, "2026-03-06T12:00:00Z"),                        
        ]);

        Assert.Equal(4, result.Summary.Received);
        Assert.Equal(1, result.Summary.Processed);
        Assert.Equal(1, result.Summary.Duplicates);
        Assert.Equal(2, result.Summary.Unattributed);
    }

    [Fact]
    public void Duplicate_events_are_not_double_counted()
    {
        var service = BuildService();

        var result = service.Recalculate(
        [
            Event("DEV-AAA", 11, "2026-03-02T08:00:00Z"),
            Event("DEV-AAA", 11, "2026-03-02T08:00:00Z"), 
        ]);

        var day = Assert.Single(result.DailyAdherence);
        Assert.Equal(1, day.DosesTaken);
    }

    [Fact]
    public void Resending_the_same_batch_is_idempotent()
    {
        var batch = new[]
        {
            Event("DEV-AAA", 11, "2026-03-02T08:00:00Z"),
            Event("DEV-AAA", 12, "2026-03-02T20:00:00Z"),
        };

        var service = BuildService();
        var first = service.Recalculate(batch);
        var second = service.Recalculate(batch); 

        Assert.Equal(2, second.Summary.Duplicates);
        Assert.Equal(0, second.Summary.Processed);
        Assert.Equal(first.DailyAdherence.Single().DosesTaken, second.DailyAdherence.Single().DosesTaken);
        Assert.Equal(first.DailyAdherence.Single().AdherenceRate, second.DailyAdherence.Single().AdherenceRate);
    }

    [Fact]
    public void Recalculation_accumulates_across_separate_batches_for_the_same_day()
    {
        var service = BuildService();

        service.Recalculate([Event("DEV-AAA", 11, "2026-03-02T08:00:00Z")]); 
        var second = service.Recalculate([Event("DEV-AAA", 12, "2026-03-02T20:00:00Z")]); 

        var day = Assert.Single(second.DailyAdherence);
        Assert.Equal(2, day.DosesTaken);
        Assert.Equal(50m, day.AdherenceRate); 
    }

    [Fact]
    public void Reassignment_attributes_events_on_the_same_device_to_different_prescriptions()
    {
        var service = BuildService();

        var result = service.Recalculate(
        [
            Event("DEV-AAA", 11, "2026-03-02T08:00:00Z"), 
            Event("DEV-AAA", 30, "2026-03-06T09:00:00Z"), 
            Event("DEV-AAA", 50, "2026-03-11T07:00:00Z"), 
        ]);

        Assert.Equal(3, result.Summary.Processed);
        Assert.Contains(result.DailyAdherence, d => d.PrescriptionId == 100 && d.PatientId == 1);
        Assert.Contains(result.DailyAdherence, d => d.PrescriptionId == 200 && d.PatientId == 1);
        Assert.Contains(result.DailyAdherence, d => d.PrescriptionId == 300 && d.PatientId == 2 && d.AdherenceRate is null);
    }

    [Fact]
    public void Each_event_gets_an_outcome_with_a_reason_when_not_processed()
    {
        var service = BuildService();

        var result = service.Recalculate([Event("DEV-ZZZ", 1, "2026-03-06T12:00:00Z")]);

        var outcome = Assert.Single(result.Outcomes);
        Assert.Equal(ProcessingStatus.Unattributed, outcome.Status);
        Assert.False(string.IsNullOrWhiteSpace(outcome.Reason));
    }

    [Fact]
    public void Null_events_throws()
    {
        Assert.Throws<ArgumentNullException>(() => BuildService().Recalculate(null!));
    }
}
