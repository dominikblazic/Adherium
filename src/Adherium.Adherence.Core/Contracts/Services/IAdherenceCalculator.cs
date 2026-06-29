using Adherium.Adherence.Core.Domain.Entities;
using Adherium.Adherence.Core.Results;

namespace Adherium.Adherence.Core.Contracts.Services;

/// <summary>Computes daily adherence summaries from stamped logs. This is the unit-tested core.</summary>
public interface IAdherenceCalculator
{
    IReadOnlyList<DailyAdherence> Calculate(IEnumerable<StampedLog> stampedLogs);
}
