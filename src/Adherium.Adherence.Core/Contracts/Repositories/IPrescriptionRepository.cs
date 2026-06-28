using Adherium.Adherence.Core.Domain.Entities;

namespace Adherium.Adherence.Core.Contracts.Repositories;

public interface IPrescriptionRepository
{
    Prescription? GetById(int id);
    IReadOnlyCollection<Prescription> GetAll();
    void Add(Prescription prescription);
}

