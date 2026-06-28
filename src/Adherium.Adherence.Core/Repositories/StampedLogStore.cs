using System.Collections.Concurrent;
using Adherium.Adherence.Core.Contracts.Repositories;
using Adherium.Adherence.Core.Domain.Entities;

namespace Adherium.Adherence.Core.Repositories;

public sealed class StampedLogRepository : IStampedLogRepository
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

        return [.. _byKey.Values.Where(l => prescriptionIds.Contains(l.PrescriptionId))];
    }
}
