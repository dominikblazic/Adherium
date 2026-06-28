using Adherium.Adherence.Core.Domain.Entities;

namespace Adherium.Adherence.Core.Contracts.Repositories;

public interface IStampedLogRepository
{
    bool TryAdd(StampedLog log);
    IReadOnlyList<StampedLog> GetForPrescriptions(IReadOnlySet<int> prescriptionIds);
}
