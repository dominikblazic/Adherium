using System.Collections.Concurrent;
using Adherium.Adherence.Core.Contracts.Repositories;
using Adherium.Adherence.Core.Domain;

namespace Adherium.Adherence.Core.Repositories;

public sealed class PrescriptionRepository : IPrescriptionRepository
{
    private readonly ConcurrentDictionary<int, Prescription> _byId = new();

    public Prescription? GetById(int id) => _byId.GetValueOrDefault(id);

    public IReadOnlyCollection<Prescription> GetAll() => _byId.Values.ToList();

    public void Add(Prescription prescription)
    {
        ArgumentNullException.ThrowIfNull(prescription);
        _byId[prescription.Id] = prescription;
    }
}
