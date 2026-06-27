using Adherium.Adherence.Core.Contracts.Repositories;
using Adherium.Adherence.Core.Contracts.Services;
using Adherium.Adherence.Core.Repositories;
using Adherium.Adherence.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Adherium.Adherence.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Stores hold state for the process lifetime, so they are singletons.
        services.AddSingleton<IPrescriptionRepository, PrescriptionRepository>();
        services.AddSingleton<IDeviceAssignmentRepository, DeviceAssignmenRepository>();
        services.AddSingleton<IStampedLogRepository, StampedLogRepository>();

        // Services are stateless.
        services.AddSingleton<IAttributionService, AttributionService>();
        services.AddSingleton<IAdherenceCalculator, AdherenceCalculator>();
        services.AddSingleton<IRecalculationService, RecalculationService>();

        return services;
    }
}
