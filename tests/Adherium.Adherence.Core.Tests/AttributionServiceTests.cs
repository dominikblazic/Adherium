using Adherium.Adherence.Core.Domain.Entities;
using Adherium.Adherence.Core.Repositories;
using Adherium.Adherence.Core.Results.Enums;
using Adherium.Adherence.Core.Services;
using static Adherium.Adherence.Core.Tests.TestData;

namespace Adherium.Adherence.Core.Tests;

public sealed class AttributionServiceTests
{
    private static AttributionService Build(
        IEnumerable<DeviceAssignment> assignments,
        IEnumerable<Prescription> prescriptions)
    {
        var assignmentStore = new DeviceAssignmenRepository();
        foreach (var a in assignments)
        {
            assignmentStore.Add(a);
        }

        var prescriptionStore = new PrescriptionRepository();
        foreach (var p in prescriptions)
        {
            prescriptionStore.Add(p);
        }

        return new AttributionService(assignmentStore, prescriptionStore);
    }

    [Fact]
    public void Event_resolves_to_the_prescription_active_at_that_instant()
    {
        var service = Build(
            [
                Assignment(1, "DEV-AAA", 100, "2026-03-01T00:00:00Z", "2026-03-04T23:59:59Z"),
                Assignment(2, "DEV-AAA", 200, "2026-03-05T00:00:00Z"),
            ],
            [Prescription(100), Prescription(200)]);

        var early = service.Resolve("DEV-AAA", Utc("2026-03-02T08:00:00Z"));
        var later = service.Resolve("DEV-AAA", Utc("2026-03-06T09:00:00Z"));

        Assert.True(early.IsAttributed);
        Assert.Equal(100, early.Prescription!.Id);
        Assert.True(later.IsAttributed);
        Assert.Equal(200, later.Prescription!.Id);
    }

    [Fact]
    public void Unknown_device_is_reported_as_such()
    {
        var service = Build([], []);

        var result = service.Resolve("DEV-ZZZ", Utc("2026-03-06T12:00:00Z"));

        Assert.False(result.IsAttributed);
        Assert.Equal(AttributionStatus.UnknownDevice, result.Status);
        Assert.Null(result.Prescription);
    }

    [Fact]
    public void Event_in_a_gap_between_assignments_is_not_attributed()
    {
        var service = Build(
            [
                Assignment(1, "DEV-AAA", 200, "2026-03-05T00:00:00Z", "2026-03-07T23:59:59Z"),
                Assignment(2, "DEV-AAA", 300, "2026-03-10T00:00:00Z"),
            ],
            [Prescription(200), Prescription(300)]);

        var result = service.Resolve("DEV-AAA", Utc("2026-03-08T10:00:00Z"));

        Assert.False(result.IsAttributed);
        Assert.Equal(AttributionStatus.NoActiveAssignment, result.Status);
    }

    [Fact]
    public void Assignment_window_is_end_inclusive()
    {
        // Sample data uses 23:59:59 end-of-day ends; the final second must still attribute.
        var service = Build(
            [Assignment(1, "DEV-AAA", 100, "2026-03-01T00:00:00Z", "2026-03-04T23:59:59Z")],
            [Prescription(100)]);

        Assert.True(service.Resolve("DEV-AAA", Utc("2026-03-04T23:59:59Z")).IsAttributed);
        Assert.False(service.Resolve("DEV-AAA", Utc("2026-03-05T00:00:00Z")).IsAttributed);
    }

    [Fact]
    public void Open_ended_assignment_covers_all_later_events()
    {
        var service = Build(
            [Assignment(1, "DEV-AAA", 200, "2026-03-05T00:00:00Z")], // no end
            [Prescription(200)]);

        Assert.True(service.Resolve("DEV-AAA", Utc("2030-01-01T00:00:00Z")).IsAttributed);
    }

    [Fact]
    public void Overlapping_windows_prefer_the_most_recently_started_assignment()
    {
        // Overlap is a data error; the tie-break is "most recent start wins".
        var service = Build(
            [
                Assignment(1, "DEV-AAA", 100, "2026-03-01T00:00:00Z", "2026-03-10T23:59:59Z"),
                Assignment(2, "DEV-AAA", 200, "2026-03-05T00:00:00Z", "2026-03-10T23:59:59Z"),
            ],
            [Prescription(100), Prescription(200)]);

        var result = service.Resolve("DEV-AAA", Utc("2026-03-06T09:00:00Z"));

        Assert.Equal(200, result.Prescription!.Id);
    }

    [Fact]
    public void Assignment_pointing_at_a_missing_prescription_is_flagged()
    {
        // Data-integrity case: the assignment exists but its prescription does not.
        var service = Build(
            [Assignment(1, "DEV-AAA", 999, "2026-03-01T00:00:00Z")],
            []);

        var result = service.Resolve("DEV-AAA", Utc("2026-03-02T08:00:00Z"));

        Assert.False(result.IsAttributed);
        Assert.Equal(AttributionStatus.MissingPrescription, result.Status);
    }
}
