using System.Collections.Concurrent;
using Adherium.Adherence.Core.Domain;

namespace Adherium.Adherence.Core.Stores;

/// <summary>
/// Stores attributed (stamped) logs. The set of stored keys doubles as the idempotency ledger:
/// an event whose <see cref="EventKey"/> is already present is a duplicate and is not re-counted.
/// </summary>
public interface IStampedLogStore
{
    /// <summary>Adds the log if its key is new. Returns <c>false</c> if it was already stored.</summary>
    bool TryAdd(StampedLog log);

    /// <summary>All stored logs for the given prescriptions.</summary>
    IReadOnlyList<StampedLog> GetForPrescriptions(IReadOnlySet<int> prescriptionIds);
}

/// <summary>In-memory stamped-log store keyed by <see cref="EventKey"/>.</summary>
public sealed class InMemoryStampedLogStore : IStampedLogStore
{
    private readonly ConcurrentDictionary<EventKey, StampedLog> _byKey = new();

    public bool TryAdd(StampedLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        return _byKey.TryAdd(log.Key, log);
    }

    public IReadOnlyList<StampedLog> GetForPrescriptions(IReadOnlySet<int> prescriptionIds)
    {
        ArgumentNullException.ThrowIfNull(prescriptionIds);
        return _byKey.Values.Where(l => prescriptionIds.Contains(l.PrescriptionId)).ToList();
    }
}
