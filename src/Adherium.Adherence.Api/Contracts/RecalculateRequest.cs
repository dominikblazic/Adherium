using System.ComponentModel.DataAnnotations;

namespace Adherium.Adherence.Api.Contracts;

public sealed record RecalculateRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one event is required.")]
    public required IReadOnlyList<LogEvent> Events { get; init; }
}

