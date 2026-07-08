using ElevatorFaultReader.Abstractions;
using ElevatorFaultReader.Configuration;
using ElevatorFaultReader.Infrastructure;
using ElevatorFaultReader.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ElevatorFaultReader;

/// <summary>
/// Modern .NET 8 console application entry point.
/// Replaces legacy static Program.Main with dependency injection and configuration.
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            // Build host with dependency injection and configuration
            var host = CreateHostBuilder(args).Build();

            // Run the application
            await RunApplicationAsync(host);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Configures host with dependency injection, configuration, and logging.
    /// </summary>
    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile(
                    $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true,
                    reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                // Register configuration options
                services.Configure<ControllerOptions>(
                    context.Configuration.GetSection(ControllerOptions.SectionName));
                services.Configure<FaultMappingOptions>(
                    context.Configuration.GetSection(FaultMappingOptions.SectionName));

                // Register services
                services.AddSingleton<IControllerDataReader, FileBasedControllerDataReader>();
                services.AddSingleton<IFaultAnalyzer, FaultAnalyzer>();
                services.AddSingleton<IFaultCodeRepository, InMemoryFaultCodeRepository>();

                // Register application
                services.AddHostedService<FaultReaderApplication>();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            });
    }

    /// <summary>
    /// Runs the application using the configured host.
    /// </summary>
    private static async Task RunApplicationAsync(IHost host)
    {
        await host.RunAsync();
    }
}

/// <summary>
/// Main application logic as a hosted service.
/// Replaces legacy static Program.Main orchestration.
/// </summary>
public sealed class FaultReaderApplication : IHostedService
{
    private readonly ILogger<FaultReaderApplication> _logger;
    private readonly IControllerDataReader _dataReader;
    private readonly IFaultAnalyzer _faultAnalyzer;
    private readonly IHostApplicationLifetime _lifetime;

    public FaultReaderApplication(
        ILogger<FaultReaderApplication> logger,
        IControllerDataReader dataReader,
        IFaultAnalyzer faultAnalyzer,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
        _faultAnalyzer = faultAnalyzer ?? throw new ArgumentNullException(nameof(faultAnalyzer));
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Read fault records from controller
            var faultRecords = await _dataReader.ReadFaultRecordsAsync(cancellationToken);

            _logger.LogInformation("Processing {Count} fault records...", faultRecords.Count);

            // Analyze each fault and display results
            foreach (var faultRecord in faultRecords)
            {
                var recommendation = _faultAnalyzer.AnalyzeFault(faultRecord);
                DisplayFaultRecommendation(faultRecord, recommendation);
            }

            _logger.LogInformation("Fault processing complete.");
        }
        catch (ControllerConnectionException ex)
        {
            _logger.LogError(ex, "Controller connection failed");
        }
        catch (ControllerDataException ex)
        {
            _logger.LogError(ex, "Controller data reading failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during fault processing");
        }
        finally
        {
            // Stop the application
            _lifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Displays fault recommendation in legacy console format.
    /// Preserves exact output format from FaultCodeService.ParseAndDisplayFault.
    /// </summary>
    private void DisplayFaultRecommendation(
        Domain.FaultRecord faultRecord,
        Domain.FaultRecommendation recommendation)
    {
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"Fault Code: {faultRecord.FaultCode}");
        Console.WriteLine($"Timestamp: {faultRecord.Timestamp}");
        Console.WriteLine($"Description: {faultRecord.Description}");
        Console.WriteLine($"Recommendation: {recommendation.RecommendationText}");

        // Add severity indicator (enhancement over legacy)
        if (recommendation.IsHighPriority)
        {
            Console.WriteLine("⚠️  HIGH PRIORITY - Immediate attention required");
        }

        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine();
    }
}