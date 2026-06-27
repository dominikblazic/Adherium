using Adherium.Adherence.Core.Domain;

namespace Adherium.Adherence.Core.Contracts.Repositories;

public interface IStampedLogRepository
{
    bool TryAdd(StampedLog log);
    IReadOnlyList<StampedLog> GetForPrescriptions(IReadOnlySet<int> prescriptionIds);
}
