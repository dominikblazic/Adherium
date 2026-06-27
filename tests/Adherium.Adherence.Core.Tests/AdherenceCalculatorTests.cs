using Adherium.Adherence.Core.Domain;
using Adherium.Adherence.Core.Repositories;
using Adherium.Adherence.Core.Services;
using static Adherium.Adherence.Core.Tests.TestData;

namespace Adherium.Adherence.Core.Tests;

/// <summary>
/// The heart of the exercise: turning stamped logs into daily adherence. These tests pin the
/// clinical rules — what counts as a dose, how the rate is computed, and how relievers differ.
/// </summary>
public sealed class AdherenceCalculatorTests
{
    private static AdherenceCalculator CalculatorFor(params Prescription[] prescriptions)
    {
        var store = new PrescriptionRepository();
        foreach (var p in prescriptions)
        {
            store.Add(p);
        }

        return new AdherenceCalculator(store);
    }

    [Fact]
    public void Controller_taking_exactly_the_schedule_is_100_percent()
    {
        // 2 doses per admin x 2 admins/day = 4 prescribed; 4 actuations taken.
        var calc = CalculatorFor(Prescription(100, dosesPerAdmin: 2, timesPerDay: 2));

        var result = calc.Calculate(
        [
            Stamped(100, "2026-03-02T08:00:00Z"),
            Stamped(100, "2026-03-02T08:01:00Z", deviceLogId: 2),
            Stamped(100, "2026-03-02T20:00:00Z", deviceLogId: 3),
            Stamped(100, "2026-03-02T20:01:00Z", deviceLogId: 4),
        ]);

        var day = Assert.Single(result);
        Assert.Equal(4, day.DosesPrescribed);
        Assert.Equal(4, day.DosesTaken);
        Assert.Equal(100m, day.AdherenceRate);
    }

    [Fact]
    public void Under_dosing_is_a_proportional_percentage()
    {
        var calc = CalculatorFor(Prescription(200, dosesPerAdmin: 1, timesPerDay: 2)); // 2 prescribed

        var result = calc.Calculate([Stamped(200, "2026-03-06T09:00:00Z")]); // 1 taken

        Assert.Equal(50m, Assert.Single(result).AdherenceRate);
    }

    [Fact]
    public void Over_dosing_is_not_capped_at_100_percent()
    {
        // Over-use is itself a clinically meaningful signal, so the rate is allowed to exceed 100.
        var calc = CalculatorFor(Prescription(100, dosesPerAdmin: 2, timesPerDay: 2)); // 4 prescribed

        var result = calc.Calculate(
        [
            Stamped(100, "2026-03-03T08:00:00Z", deviceLogId: 1),
            Stamped(100, "2026-03-03T08:01:00Z", deviceLogId: 2),
            Stamped(100, "2026-03-03T08:02:00Z", deviceLogId: 3),
            Stamped(100, "2026-03-03T20:00:00Z", deviceLogId: 4),
            Stamped(100, "2026-03-03T20:05:00Z", deviceLogId: 5),
        ]); // 5 taken

        Assert.Equal(125m, Assert.Single(result).AdherenceRate);
    }

    [Fact]
    public void Rate_is_rounded_to_two_decimal_places()
    {
        var calc = CalculatorFor(Prescription(100, dosesPerAdmin: 1, timesPerDay: 3)); // 3 prescribed

        var result = calc.Calculate([Stamped(100, "2026-03-02T08:00:00Z")]); // 1/3 = 33.333...

        Assert.Equal(33.33m, Assert.Single(result).AdherenceRate);
    }

    [Fact]
    public void Relievers_report_usage_but_no_adherence_rate()
    {
        // PRN rescue medication: "percent of scheduled doses" is meaningless, so rate is null.
        var calc = CalculatorFor(Prescription(300, type: MedicationType.Reliever, patientId: 2));

        var day = Assert.Single(calc.Calculate(
        [
            Stamped(300, "2026-03-11T07:00:00Z", patientId: 2),
            Stamped(300, "2026-03-11T19:00:00Z", patientId: 2, deviceLogId: 2),
        ]));

        Assert.Null(day.AdherenceRate);
        Assert.Equal(2, day.DosesTaken); // usage is still surfaced
    }

    [Fact]
    public void Only_actuations_count_as_doses()
    {
        // PeakInhalationFlow / Unknown are stamped for traceability but never counted.
        var calc = CalculatorFor(Prescription(200, dosesPerAdmin: 1, timesPerDay: 2));

        var day = Assert.Single(calc.Calculate(
        [
            Stamped(200, "2026-03-06T09:00:00Z", EventType.Actuation, deviceLogId: 1),
            Stamped(200, "2026-03-06T09:05:00Z", EventType.PeakInhalationFlow, deviceLogId: 2),
            Stamped(200, "2026-03-06T09:06:00Z", EventType.Unknown, deviceLogId: 3),
        ]));

        Assert.Equal(1, day.DosesTaken);
    }

    [Fact]
    public void Events_are_bucketed_into_one_summary_per_prescription_and_utc_day()
    {
        var calc = CalculatorFor(Prescription(100, dosesPerAdmin: 2, timesPerDay: 2));

        var result = calc.Calculate(
        [
            Stamped(100, "2026-03-02T08:00:00Z", deviceLogId: 1),
            Stamped(100, "2026-03-02T20:00:00Z", deviceLogId: 2),
            Stamped(100, "2026-03-03T08:00:00Z", deviceLogId: 3),
        ]);

        Assert.Equal(2, result.Count);
        Assert.Equal(new DateOnly(2026, 3, 2), result[0].Date);
        Assert.Equal(2, result[0].DosesTaken);
        Assert.Equal(new DateOnly(2026, 3, 3), result[1].Date);
        Assert.Equal(1, result[1].DosesTaken);
    }

    [Fact]
    public void Day_boundary_is_utc_so_late_evening_and_next_morning_split()
    {
        var calc = CalculatorFor(Prescription(100));

        var result = calc.Calculate(
        [
            Stamped(100, "2026-03-02T23:30:00Z", deviceLogId: 1), // still the 2nd in UTC
            Stamped(100, "2026-03-03T00:30:00Z", deviceLogId: 2), // now the 3rd in UTC
        ]);

        Assert.Equal(2, result.Count);
        Assert.Equal(new DateOnly(2026, 3, 2), result[0].Date);
        Assert.Equal(new DateOnly(2026, 3, 3), result[1].Date);
    }

    [Fact]
    public void Results_are_ordered_by_patient_then_prescription_then_date()
    {
        var calc = CalculatorFor(
            Prescription(100, patientId: 1),
            Prescription(300, type: MedicationType.Reliever, patientId: 2));

        var result = calc.Calculate(
        [
            Stamped(300, "2026-03-11T07:00:00Z", patientId: 2, deviceLogId: 1),
            Stamped(100, "2026-03-03T08:00:00Z", patientId: 1, deviceLogId: 2),
            Stamped(100, "2026-03-02T08:00:00Z", patientId: 1, deviceLogId: 3),
        ]);

        Assert.Collection(result,
            r => Assert.Equal((1, 100, new DateOnly(2026, 3, 2)), (r.PatientId, r.PrescriptionId, r.Date)),
            r => Assert.Equal((1, 100, new DateOnly(2026, 3, 3)), (r.PatientId, r.PrescriptionId, r.Date)),
            r => Assert.Equal((2, 300, new DateOnly(2026, 3, 11)), (r.PatientId, r.PrescriptionId, r.Date)));
    }

    [Fact]
    public void No_logs_produces_no_summaries()
    {
        Assert.Empty(CalculatorFor().Calculate([]));
    }

    [Fact]
    public void Null_logs_throws()
    {
        Assert.Throws<ArgumentNullException>(() => CalculatorFor().Calculate(null!));
    }
}
