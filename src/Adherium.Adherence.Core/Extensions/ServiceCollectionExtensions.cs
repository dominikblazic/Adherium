using Adherium.Adherence.Core.Contracts;
using Adherium.Adherence.Core.Services;
using Adherium.Adherence.Core.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace Adherium.Adherence.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Stores hold state for the process lifetime, so they are singletons.
        services.AddSingleton<IPrescriptionStore, InMemoryPrescriptionStore>();
        services.AddSingleton<IDeviceAssignmentStore, InMemoryDeviceAssignmentStore>();
        services.AddSingleton<IStampedLogStore, InMemoryStampedLogStore>();

        // Services are stateless.
        services.AddSingleton<IAttributionService, AttributionService>();
        services.AddSingleton<IAdherenceCalculator, AdherenceCalculator>();
        services.AddSingleton<IRecalculationService, RecalculationService>();

        return services;
    }
}
