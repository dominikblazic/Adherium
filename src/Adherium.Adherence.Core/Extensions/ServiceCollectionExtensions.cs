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

        services.AddSingleton<IPrescriptionRepository, PrescriptionRepository>();
        services.AddSingleton<IDeviceAssignmentRepository, DeviceAssignmenRepository>();
        services.AddSingleton<IStampedLogRepository, StampedLogRepository>();

        services.AddScoped<IAttributionService, AttributionService>();
        services.AddScoped<IAdherenceCalculator, AdherenceCalculator>();
        services.AddScoped<IRecalculationService, RecalculationService>();

        return services;
    }
}
