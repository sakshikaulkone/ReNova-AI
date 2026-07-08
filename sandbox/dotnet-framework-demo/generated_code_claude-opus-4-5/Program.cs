using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModernElevatorFaultReader.Abstractions;
using ModernElevatorFaultReader.Configuration;
using ModernElevatorFaultReader.Infrastructure;
using ModernElevatorFaultReader.Presentation;
using ModernElevatorFaultReader.Services;

namespace ModernElevatorFaultReader;

/// <summary>
/// Modern .NET 8 console application entry point using dependency injection,
/// configuration management, and structured logging.
/// Replaces legacy Program.cs with hardcoded file paths and static method calls.
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            // Build host with dependency injection and configuration
            var host = CreateHostBuilder(args).Build();

            // Resolve application service and run
            var app = host.Services.GetRequiredService<FaultReaderApplication>();
            await app.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                    optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                // Register configuration options
                services.Configure<ControllerOptions>(
                    context.Configuration.GetSection(ControllerOptions.SectionName));

                // Register application services
                services.AddSingleton<IFaultDataReader, TextFileFaultDataReader>();
                services.AddSingleton<IFaultAnalyzer, FaultAnalyzer>();
                services.AddSingleton<ConsoleFaultPresenter>();
                services.AddSingleton<FaultReaderApplication>();

                // Configure logging
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
            });
}

/// <summary>
/// Main application orchestration class.
/// Coordinates fault reading, analysis, and presentation.
/// </summary>
public class FaultReaderApplication
{
    private readonly IFaultDataReader _faultDataReader;
    private readonly IFaultAnalyzer _faultAnalyzer;
    private readonly ConsoleFaultPresenter _presenter;
    private readonly ILogger<FaultReaderApplication> _logger;

    public FaultReaderApplication(
        IFaultDataReader faultDataReader,
        IFaultAnalyzer faultAnalyzer,
        ConsoleFaultPresenter presenter,
        ILogger<FaultReaderApplication> logger)
    {
        _faultDataReader = faultDataReader ?? throw new ArgumentNullException(nameof(faultDataReader));
        _faultAnalyzer = faultAnalyzer ?? throw new ArgumentNullException(nameof(faultAnalyzer));
        _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunAsync()
    {
        _presenter.DisplayHeader();

        try
        {
            // Read fault records from data source
            _logger.LogInformation("Reading fault data from controller");
            var faultRecords = await _faultDataReader.ReadFaultRecordsAsync();

            _logger.LogInformation("Successfully read {Count} fault records", faultRecords.Count);
            _presenter.DisplayRecordCount(faultRecords.Count);

            // Analyze each fault and get recommendations
            foreach (var faultRecord in faultRecords)
            {
                var recommendation = _faultAnalyzer.AnalyzeFault(faultRecord);
                _presenter.DisplayFaultWithRecommendation(faultRecord, recommendation);
            }

            _presenter.DisplayFooter();
            _logger.LogInformation("Fault processing completed successfully");
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Controller data file not found");
            _presenter.DisplayError($"Controller data file not found: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing fault data");
            _presenter.DisplayError($"Error processing fault data: {ex.Message}");
            throw;
        }
    }
}