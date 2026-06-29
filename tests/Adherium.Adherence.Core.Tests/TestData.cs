using Adherium.Adherence.Core.Domain.Entities;
using Adherium.Adherence.Core.Domain.Enums;
using Adherium.Adherence.Core.Domain.Models;

namespace Adherium.Adherence.Core.Tests;

internal static class TestData
{
    public static DateTimeOffset Utc(string iso) => DateTimeOffset.Parse(
        iso, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal);

    public static Prescription Prescription(
        int id,
        MedicationType type = MedicationType.Controller,
        int dosesPerAdmin = 2,
        int timesPerDay = 2,
        int patientId = 1,
        string start = "2026-03-01T00:00:00Z",
        string? end = null) => new()
    {
        Id = id,
        PatientId = patientId,
        MedicationType = type,
        DosesPerAdmin = dosesPerAdmin,
        TimesPerDay = timesPerDay,
        StartUtc = Utc(start),
        EndUtc = end is null ? null : Utc(end),
    };

    public static DeviceAssignment Assignment(
        int id,
        string serial,
        int prescriptionId,
        string start,
        string? end = null) => new()
    {
        Id = id,
        DeviceSerial = serial,
        PrescriptionId = prescriptionId,
        StartUtc = Utc(start),
        EndUtc = end is null ? null : Utc(end),
    };

    public static StampedLog Stamped(
        int prescriptionId,
        string timestamp,
        EventType type = EventType.Actuation,
        int patientId = 1,
        string serial = "DEV-AAA",
        int deviceLogId = 1) => new()
    {
        DeviceSerial = serial,
        DeviceLogId = deviceLogId,
        EventTimestampUtc = Utc(timestamp),
        EventType = type,
        PrescriptionId = prescriptionId,
        PatientId = patientId,
    };

    public static LogEvent Event(
        string serial,
        int deviceLogId,
        string timestamp,
        string eventType = "Actuation") => new()
    {
        DeviceSerial = serial,
        DeviceLogId = deviceLogId,
        EventTimestampUtc = Utc(timestamp),
        EventType = eventType,
    };
}
