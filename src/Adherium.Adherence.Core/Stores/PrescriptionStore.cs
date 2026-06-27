using System.Collections.Concurrent;
using Adherium.Adherence.Core.Domain;

namespace Adherium.Adherence.Core.Stores;

/// <summary>Read access to the seeded prescriptions.</summary>
public interface IPrescriptionStore
{
    Prescription? GetById(int id);
    IReadOnlyCollection<Prescription> GetAll();
    void Add(Prescription prescription);
}

/// <summary>In-memory prescription store. Swap for a repository when persistence is added.</summary>
public sealed class InMemoryPrescriptionStore : IPrescriptionStore
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
