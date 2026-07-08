using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernElevatorFaultReader.Abstractions;
using ModernElevatorFaultReader.Configuration;
using ModernElevatorFaultReader.Services;

namespace ModernElevatorFaultReader;

/// <summary>
/// Extension methods for registering fault processing services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all fault processing services with dependency injection container.
    /// </summary>
    public static IServiceCollection AddFaultProcessingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register configuration options
        services.Configure<FaultCodeOptions>(
            configuration.GetSection(FaultCodeOptions.SectionName));

        // Register service implementations
        services.AddSingleton<IFaultDataParser, FaultDataParser>();
        services.AddSingleton<IFaultAnalyzer, FaultAnalyzer>();
        services.AddSingleton<IFaultOutputFormatter, ConsoleFaultOutputFormatter>();

        return services;
    }
}