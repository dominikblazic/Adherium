using Adherium.Adherence.Core.Domain.Entities;
using Adherium.Adherence.Core.Results;

namespace Adherium.Adherence.Core.Contracts.Services;

public interface IAdherenceCalculator
{
    IReadOnlyList<DailyAdherence> Calculate(IEnumerable<StampedLog> stampedLogs);
}
