using Microsoft.Extensions.Logging;

namespace Adherium.Adherence.Core.Extensions;

public static partial class LogMessageExtensions
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Sample data file not found at {FilePath}; stores left empty.")]
    public static partial void LogSampleFileMissing(this ILogger logger, string filePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded {PrescriptionCount} prescriptions and {AssignmentCount} device assignments.")]
    public static partial void LogSeeded(this ILogger logger, int prescriptionCount, int assignmentCount);
}
