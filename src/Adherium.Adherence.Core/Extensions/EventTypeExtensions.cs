using Adherium.Adherence.Core.Domain.Enums;

namespace Adherium.Adherence.Core.Extensions;

public static class EventTypeExtensions
{
    /// <summary>Parses a raw event-type string, falling back to <see cref="EventType.Unknown"/>.</summary>
    public static EventType ToEventType(this string? raw) =>
        Enum.TryParse<EventType>(raw, ignoreCase: true, out var parsed) ? parsed : EventType.Unknown;
}
