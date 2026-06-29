using Adherium.Adherence.Core.Domain.Models;
using Adherium.Adherence.Core.Results;

namespace Adherium.Adherence.Core.Contracts.Services;

public interface IRecalculationService
{
    RecalculationResult Recalculate(IReadOnlyCollection<LogEvent> events);
}
