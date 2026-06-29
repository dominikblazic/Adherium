using Adherium.Adherence.Core.Domain.Entities;
using Adherium.Adherence.Core.Results.Enums;

namespace Adherium.Adherence.Core.Results;

public readonly record struct AttributionResult(AttributionStatus Status, Prescription? Prescription)
{
    public bool IsAttributed => Status == AttributionStatus.Attributed && Prescription is not null;

    public static AttributionResult Attributed(Prescription prescription) =>
        new(AttributionStatus.Attributed, prescription);

    public static AttributionResult Failed(AttributionStatus status) => new(status, null);
}
