using Adherium.Adherence.Core.Results;

namespace Adherium.Adherence.Core.Contracts.Services;

public interface IAttributionService
{
    AttributionResult Resolve(string deviceSerial, DateTimeOffset eventTimestampUtc);
}

