using Adherium.Adherence.Core.Domain;
using Adherium.Adherence.Core.Results;

namespace Adherium.Adherence.Core.Contracts;

/// <summary>Computes daily adherence summaries from stamped logs. This is the unit-tested core.</summary>
public interface IAdherenceCalculator
{
    IReadOnlyList<DailyAdherence> Calculate(IEnumerable<StampedLog> stampedLogs);
}
