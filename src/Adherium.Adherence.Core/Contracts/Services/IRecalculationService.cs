using Adherium.Adherence.Core.Domain.Dtos.Results;
using Adherium.Adherence.Core.Domain.Models;

namespace Adherium.Adherence.Core.Contracts.Services;

/// <summary>Orchestrates a batch: attribute → stamp (idempotently) → recalculate affected days.</summary>
public interface IRecalculationService
{
    RecalculationResult Recalculate(IReadOnlyCollection<LogEvent> events);
}
