using System.ComponentModel.DataAnnotations;
using Adherium.Adherence.Core.Domain;

namespace Adherium.Adherence.Api.Contracts;

/// <summary>Request body for the recalculate endpoint: a batch of sensor log events.</summary>
public sealed record RecalculateRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one event is required.")]
    public required IReadOnlyList<LogEventContract> Events { get; init; }
}

/// <summary>A single incoming sensor log event (API surface, validated and mapped to the domain).</summary>
public sealed record LogEventContract
{
    [Required(AllowEmptyStrings = false)]
    public required string DeviceSerial { get; init; }

    public required int DeviceLogId { get; init; }

    public required DateTimeOffset EventTimestampUtc { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string EventType { get; init; }

    public LogEvent ToDomain() => new()
    {
        DeviceSerial = DeviceSerial,
        DeviceLogId = DeviceLogId,
        EventTimestampUtc = EventTimestampUtc,
        EventType = EventType,
    };
}
