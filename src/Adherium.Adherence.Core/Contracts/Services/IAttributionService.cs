using Adherium.Adherence.Core.Services;

namespace Adherium.Adherence.Core.Contracts.Services;

/// <summary>Resolves which prescription was active for a device at a given instant.</summary>
public interface IAttributionService
{
    AttributionResult Resolve(string deviceSerial, DateTimeOffset eventTimestampUtc);
}

