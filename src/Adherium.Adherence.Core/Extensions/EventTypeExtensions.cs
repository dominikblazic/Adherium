using Adherium.Adherence.Core.Domain.Enums;

namespace Adherium.Adherence.Core.Extensions;

public static class EventTypeExtensions
{
    public static EventType ToEventType(this string? raw) =>
        Enum.TryParse<EventType>(raw, ignoreCase: true, out var parsed) ? parsed : EventType.Unknown;
}
