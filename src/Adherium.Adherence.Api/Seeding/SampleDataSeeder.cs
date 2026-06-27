using System.Text.Json;
using System.Text.Json.Serialization;
using Adherium.Adherence.Core.Domain;
using Adherium.Adherence.Core.Stores;

namespace Adherium.Adherence.Api.Seeding;

/// <summary>
/// Seeds the in-memory prescription and device-assignment stores from the synthetic sample file
/// (<c>sample-batch.json</c>, copied next to the app). The file's <c>batch.events</c> are not seeded —
/// those are what you POST to the endpoint.
/// </summary>
public sealed partial class SampleDataSeeder(
    IPrescriptionStore prescriptions,
    IDeviceAssignmentStore assignments,
    ILogger<SampleDataSeeder> logger)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public void Seed(string filePath)
    {
        if (!File.Exists(filePath))
        {
            LogSampleFileMissing(filePath);
            return;
        }

        var json = File.ReadAllText(filePath);
        var document = JsonSerializer.Deserialize<SampleDataDocument>(json, s_jsonOptions)
            ?? throw new InvalidOperationException($"Sample data at '{filePath}' could not be parsed.");

        foreach (var prescription in document.Prescriptions)
        {
            prescriptions.Add(prescription.ToDomain());
        }

        foreach (var assignment in document.DeviceAssignments)
        {
            assignments.Add(assignment.ToDomain());
        }

        LogSeeded(document.Prescriptions.Count, document.DeviceAssignments.Count);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sample data file not found at {FilePath}; stores left empty.")]
    private partial void LogSampleFileMissing(string filePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded {PrescriptionCount} prescriptions and {AssignmentCount} device assignments.")]
    private partial void LogSeeded(int prescriptionCount, int assignmentCount);

    private sealed record SampleDataDocument
    {
        public IReadOnlyList<PrescriptionSeed> Prescriptions { get; init; } = [];
        public IReadOnlyList<DeviceAssignmentSeed> DeviceAssignments { get; init; } = [];
    }

    private sealed record PrescriptionSeed(
        int Id,
        int PatientId,
        MedicationType MedicationType,
        int DosesPerAdmin,
        int TimesPerDay,
        DateTimeOffset StartUtc,
        DateTimeOffset? EndUtc)
    {
        public Prescription ToDomain() => new()
        {
            Id = Id,
            PatientId = PatientId,
            MedicationType = MedicationType,
            DosesPerAdmin = DosesPerAdmin,
            TimesPerDay = TimesPerDay,
            StartUtc = StartUtc,
            EndUtc = EndUtc,
        };
    }

    private sealed record DeviceAssignmentSeed(
        int Id,
        string DeviceSerial,
        int PrescriptionId,
        DateTimeOffset StartUtc,
        DateTimeOffset? EndUtc)
    {
        public DeviceAssignment ToDomain() => new()
        {
            Id = Id,
            DeviceSerial = DeviceSerial,
            PrescriptionId = PrescriptionId,
            StartUtc = StartUtc,
            EndUtc = EndUtc,
        };
    }
}
