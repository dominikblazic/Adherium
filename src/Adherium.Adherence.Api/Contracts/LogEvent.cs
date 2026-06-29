using System.ComponentModel.DataAnnotations;
using DomainLogEvent = Adherium.Adherence.Core.Domain.Models.LogEvent;

namespace Adherium.Adherence.Api.Contracts;

public sealed record LogEvent
{
    [Required(AllowEmptyStrings = false)]
    public required string DeviceSerial { get; init; }

    public required int DeviceLogId { get; init; }

    public required DateTimeOffset EventTimestampUtc { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string EventType { get; init; }

    public DomainLogEvent ToDomain() => new()
    {
        DeviceSerial = DeviceSerial,
        DeviceLogId = DeviceLogId,
        EventTimestampUtc = EventTimestampUtc,
        EventType = EventType,
    };
}

