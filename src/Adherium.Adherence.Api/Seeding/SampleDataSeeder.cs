using System.Text.Json;
using System.Text.Json.Serialization;
using Adherium.Adherence.Core.Contracts.Repositories;
using Adherium.Adherence.Core.Domain.Entities;
using Adherium.Adherence.Core.Domain.Enums;
using Adherium.Adherence.Core.Extensions;

namespace Adherium.Adherence.Api.Seeding;

public sealed class SampleDataSeeder(
    IPrescriptionRepository prescriptionRepository,
    IDeviceAssignmentRepository deviceAssignmentRepository,
    ILogger<SampleDataSeeder> logger)
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public void Seed(string filePath)
    {
        if (!File.Exists(filePath))
        {
            logger.LogSampleFileMissing(filePath);
            return;
        }

        var json = File.ReadAllText(filePath);
        var document = JsonSerializer.Deserialize<SampleDataDocument>(json, _jsonOptions)
            ?? throw new InvalidOperationException($"Sample data at '{filePath}' could not be parsed.");

        foreach (var prescription in document.Prescriptions)
        {
            prescriptionRepository.Add(prescription.ToDomain());
        }

        foreach (var assignment in document.DeviceAssignments)
        {
            deviceAssignmentRepository.Add(assignment.ToDomain());
        }

        logger.LogSeeded(document.Prescriptions.Count, document.DeviceAssignments.Count);
    }

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
