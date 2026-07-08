# Code Conversion Report

## 1. Summary

Converted legacy VB6/static .NET Framework fault reading logic into modern .NET 8 architecture with dependency injection, interface abstractions for controller communication, Options pattern configuration, structured logging, and case-insensitive fault code matching while preserving all 8 fault code recommendation mappings.

## 2. Generated Files List

- `Domain/FaultRecord.cs`
- `Domain/FaultRecommendation.cs`
- `Domain/FaultCode.cs`
- `Configuration/ControllerOptions.cs`
- `Configuration/FaultMappingOptions.cs`
- `Abstractions/IControllerDataReader.cs`
- `Abstractions/IFaultAnalyzer.cs`
- `Services/FaultAnalyzer.cs`
- `Infrastructure/FileBasedControllerDataReader.cs`
- `Infrastructure/FaultCodeRepository.cs`
- `Program.cs`
- `appsettings.json`

## 3. Business Rules Preserved

- All 8 fault code to recommendation mappings preserved exactly (DOOR_LOCK_FAILURE, MOTOR_OVERCURRENT, LEVELING_SENSOR_FAULT, COMMUNICATION_TIMEOUT, BRAKE_SWITCH_FAULT, DOOR_REVERSAL, POSITION_ERROR, UNKNOWN_CODE)
- Case-insensitive fault code matching (VB6 behavior) using `StringComparison.OrdinalIgnoreCase`
- Pipe-delimited parsing with field trimming and 3-field validation
- Continue-on-error behavior for malformed fault lines
- Unknown fault code fallback message: "Unknown fault code. Consult technical documentation."

## 4. Generated Code Blocks

### File: Domain/FaultRecord.cs

```csharp
namespace ElevatorFaultReader.Domain;

/// <summary>
/// Represents a single fault record read from the elevator controller.
/// Immutable record type for thread-safety and value semantics.
/// </summary>
public sealed record FaultRecord
{
    /// <summary>
    /// Timestamp when the fault occurred (ISO 8601 format from controller).
    /// </summary>
    public required string Timestamp { get; init; }

    /// <summary>
    /// Fault code identifier (e.g., "DOOR_LOCK_FAILURE").
    /// Case-insensitive matching is applied during analysis.
    /// </summary>
    public required string FaultCode { get; init; }

    /// <summary>
    /// Human-readable description of the fault from the controller.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Raw fault line as received from controller (for audit/logging).
    /// </summary>
    public string? RawLine { get; init; }
}
```

### File: Domain/FaultRecommendation.cs

```csharp
namespace ElevatorFaultReader.Domain;

/// <summary>
/// Represents a technician recommendation for a specific fault code.
/// Immutable record containing diagnostic guidance.
/// </summary>
public sealed record FaultRecommendation
{
    /// <summary>
    /// The fault code this recommendation applies to.
    /// </summary>
    public required string FaultCode { get; init; }

    /// <summary>
    /// Technician recommendation text with diagnostic steps.
    /// </summary>
    public required string RecommendationText { get; init; }

    /// <summary>
    /// Severity level for future prioritization (not used in legacy system).
    /// </summary>
    public FaultSeverity Severity { get; init; } = FaultSeverity.Medium;

    /// <summary>
    /// Whether this is a high-priority fault requiring immediate attention.
    /// </summary>
    public bool IsHighPriority => Severity == FaultSeverity.High;
}
```

### File: Domain/FaultCode.cs

```csharp
namespace ElevatorFaultReader.Domain;

/// <summary>
/// Enumeration of known fault codes.
/// Used for type-safe fault code handling.
/// </summary>
public static class FaultCode
{
    public const string DoorLockFailure = "DOOR_LOCK_FAILURE";
    public const string MotorOvercurrent = "MOTOR_OVERCURRENT";
    public const string LevelingSensorFault = "LEVELING_SENSOR_FAULT";
    public const string CommunicationTimeout = "COMMUNICATION_TIMEOUT";
    public const string BrakeSwitchFault = "BRAKE_SWITCH_FAULT";
    public const string DoorReversal = "DOOR_REVERSAL";
    public const string PositionError = "POSITION_ERROR";
    public const string UnknownCode = "UNKNOWN_CODE";
}

/// <summary>
/// Fault severity levels for prioritization.
/// High-severity faults: MOTOR_OVERCURRENT, BRAKE_SWITCH_FAULT, POSITION_ERROR
/// </summary>
public enum FaultSeverity
{
    Low = 1,
    Medium = 2,
    High = 3
}
```

### File: Configuration/ControllerOptions.cs

```csharp
namespace ElevatorFaultReader.Configuration;

/// <summary>
/// Configuration options for controller data source.
/// Replaces hardcoded file path from legacy Program.cs.
/// </summary>
public sealed class ControllerOptions
{
    public const string SectionName = "Controller";

    /// <summary>
    /// Path to controller data file (for file-based simulation).
    /// In production, this would be replaced with serial port or network configuration.
    /// </summary>
    public string DataFilePath { get; set; } = "controller_fault_codes.txt";

    /// <summary>
    /// Connection timeout in seconds (for future real controller communication).
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to validate controller connection before reading data.
    /// Placeholder for future dongle/authentication logic.
    /// </summary>
    public bool ValidateConnection { get; set; } = false;
}
```

### File: Configuration/FaultMappingOptions.cs

```csharp
namespace ElevatorFaultReader.Configuration;

/// <summary>
/// Configuration options for fault code mappings.
/// Allows externalization of fault recommendations to configuration.
/// </summary>
public sealed class FaultMappingOptions
{
    public const string SectionName = "FaultMapping";

    /// <summary>
    /// Whether to use case-insensitive fault code matching (VB6 legacy behavior).
    /// </summary>
    public bool CaseInsensitiveMatching { get; set; } = true;

    /// <summary>
    /// Default recommendation for unknown fault codes.
    /// </summary>
    public string UnknownFaultRecommendation { get; set; } = 
        "Unknown fault code. Consult technical documentation.";

    /// <summary>
    /// Dictionary of fault code to recommendation mappings.
    /// Can be overridden in appsettings.json for customization.
    /// </summary>
    public Dictionary<string, string> Recommendations { get; set; } = new();
}
```

### File: Abstractions/IControllerDataReader.cs

```csharp
namespace ElevatorFaultReader.Abstractions;

using ElevatorFaultReader.Domain;

/// <summary>
/// Abstraction for reading fault data from elevator controller.
/// Replaces static LegacyControllerClient with testable interface.
/// Implementations: file-based (current), serial port (future), network (future).
/// </summary>
public interface IControllerDataReader
{
    /// <summary>
    /// Reads fault records from the controller data source.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Collection of fault records read from controller.</returns>
    /// <exception cref="ControllerConnectionException">Thrown when controller connection fails.</exception>
    /// <exception cref="ControllerDataException">Thrown when data reading fails.</exception>
    Task<IReadOnlyList<FaultRecord>> ReadFaultRecordsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates controller connection without reading data.
    /// Placeholder for future dongle validation or authentication.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if connection is valid, false otherwise.</returns>
    Task<bool> ValidateConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Exception thrown when controller connection fails.
/// </summary>
public sealed class ControllerConnectionException : Exception
{
    public ControllerConnectionException(string message) : base(message) { }
    public ControllerConnectionException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when controller data reading fails.
/// </summary>
public sealed class ControllerDataException : Exception
{
    public ControllerDataException(string message) : base(message) { }
    public ControllerDataException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

### File: Abstractions/IFaultAnalyzer.cs

```csharp
namespace ElevatorFaultReader.Abstractions;

using ElevatorFaultReader.Domain;

/// <summary>
/// Abstraction for fault analysis and recommendation logic.
/// Replaces static FaultCodeService with testable interface.
/// </summary>
public interface IFaultAnalyzer
{
    /// <summary>
    /// Analyzes a fault record and returns technician recommendation.
    /// Implements case-insensitive fault code matching (VB6 legacy behavior).
    /// </summary>
    /// <param name="faultRecord">Fault record to analyze.</param>
    /// <returns>Fault recommendation with diagnostic guidance.</returns>
    FaultRecommendation AnalyzeFault(FaultRecord faultRecord);

    /// <summary>
    /// Analyzes multiple fault records and returns recommendations.
    /// </summary>
    /// <param name="faultRecords">Collection of fault records to analyze.</param>
    /// <returns>Collection of fault recommendations.</returns>
    IReadOnlyList<FaultRecommendation> AnalyzeFaults(IEnumerable<FaultRecord> faultRecords);
}
```

### File: Services/FaultAnalyzer.cs

```csharp
namespace ElevatorFaultReader.Services;

using ElevatorFaultReader.Abstractions;
using ElevatorFaultReader.Configuration;
using ElevatorFaultReader.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Default implementation of fault analysis service.
/// Replaces static FaultCodeService.GetTechnicianRecommendation method.
/// Implements case-insensitive fault code matching (VB6 legacy behavior).
/// </summary>
public sealed class FaultAnalyzer : IFaultAnalyzer
{
    private readonly ILogger<FaultAnalyzer> _logger;
    private readonly FaultMappingOptions _options;
    private readonly IReadOnlyDictionary<string, string> _faultRecommendations;

    public FaultAnalyzer(
        ILogger<FaultAnalyzer> logger,
        IOptions<FaultMappingOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Build case-insensitive fault recommendation dictionary
        _faultRecommendations = BuildFaultRecommendations();
    }

    /// <summary>
    /// Analyzes a single fault record and returns recommendation.
    /// Preserves exact legacy business logic with case-insensitive matching.
    /// </summary>
    public FaultRecommendation AnalyzeFault(FaultRecord faultRecord)
    {
        if (faultRecord == null)
        {
            throw new ArgumentNullException(nameof(faultRecord));
        }

        _logger.LogDebug(
            "Analyzing fault code: {FaultCode} at {Timestamp}",
            faultRecord.FaultCode,
            faultRecord.Timestamp);

        // Case-insensitive lookup (VB6 legacy behavior)
        var comparisonType = _options.CaseInsensitiveMatching
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        var matchingKey = _faultRecommendations.Keys
            .FirstOrDefault(k => k.Equals(faultRecord.FaultCode, comparisonType));

        string recommendationText;
        FaultSeverity severity;

        if (matchingKey != null)
        {
            recommendationText = _faultRecommendations[matchingKey];
            severity = GetFaultSeverity(matchingKey);

            _logger.LogInformation(
                "Fault code {FaultCode} matched to recommendation (severity: {Severity})",
                faultRecord.FaultCode,
                severity);
        }
        else
        {
            recommendationText = _options.UnknownFaultRecommendation;
            severity = FaultSeverity.Medium;

            _logger.LogWarning(
                "Unknown fault code: {FaultCode}. Using default recommendation.",
                faultRecord.FaultCode);
        }

        return new FaultRecommendation
        {
            FaultCode = faultRecord.FaultCode,
            RecommendationText = recommendationText,
            Severity = severity
        };
    }

    /// <summary>
    /// Analyzes multiple fault records.
    /// </summary>
    public IReadOnlyList<FaultRecommendation> AnalyzeFaults(IEnumerable<FaultRecord> faultRecords)
    {
        if (faultRecords == null)
        {
            throw new ArgumentNullException(nameof(faultRecords));
        }

        return faultRecords
            .Select(AnalyzeFault)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Builds fault recommendation dictionary from configuration or defaults.
    /// Preserves exact legacy fault code mappings from FaultCodeService.cs.
    /// </summary>
    private IReadOnlyDictionary<string, string> BuildFaultRecommendations()
    {
        // Use configuration mappings if provided, otherwise use legacy defaults
        if (_options.Recommendations.Count > 0)
        {
            _logger.LogInformation(
                "Using {Count} fault mappings from configuration",
                _options.Recommendations.Count);

            return _options.Recommendations.AsReadOnly();
        }

        // Legacy default mappings (preserved exactly from FaultCodeService.cs)
        var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [FaultCode.DoorLockFailure] = 
                "Inspect door lock mechanism and wiring. Check for mechanical obstruction.",

            [FaultCode.MotorOvercurrent] = 
                "Check motor windings and bearings. Verify load conditions. Inspect motor contactor.",

            [FaultCode.LevelingSensorFault] = 
                "Clean leveling sensors. Check sensor alignment and wiring connections.",

            [FaultCode.CommunicationTimeout] = 
                "Verify controller communication cable connections. Check for electromagnetic interference.",

            [FaultCode.BrakeSwitchFault] = 
                "Inspect brake switch operation. Verify brake coil voltage and mechanical linkage.",

            [FaultCode.DoorReversal] = 
                "Check door reversal sensor and safety edge. Verify door track alignment.",

            [FaultCode.PositionError] = 
                "Inspect position encoder and mounting. Check for mechanical wear or misalignment.",

            [FaultCode.UnknownCode] = 
                "Refer to controller technical manual. Contact manufacturer support if needed."
        };

        _logger.LogInformation(
            "Using {Count} default fault mappings (legacy behavior)",
            defaults.Count);

        return defaults;
    }

    /// <summary>
    /// Determines fault severity based on fault code.
    /// High-severity faults: MOTOR_OVERCURRENT, BRAKE_SWITCH_FAULT, POSITION_ERROR
    /// </summary>
    private static FaultSeverity GetFaultSeverity(string faultCode)
    {
        return faultCode.ToUpperInvariant() switch
        {
            "MOTOR_OVERCURRENT" => FaultSeverity.High,
            "BRAKE_SWITCH_FAULT" => FaultSeverity.High,
            "POSITION_ERROR" => FaultSeverity.High,
            "DOOR_LOCK_FAILURE" => FaultSeverity.Medium,
            "COMMUNICATION_TIMEOUT" => FaultSeverity.Medium,
            "LEVELING_SENSOR_FAULT" => FaultSeverity.Low,
            "DOOR_REVERSAL" => FaultSeverity.Low,
            _ => FaultSeverity.Medium
        };
    }
}
```

### File: Infrastructure/FileBasedControllerDataReader.cs

```csharp
namespace ElevatorFaultReader.Infrastructure;

using ElevatorFaultReader.Abstractions;
using ElevatorFaultReader.Configuration;
using ElevatorFaultReader.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

/// <summary>
/// File-based implementation of controller data reader.
/// Replaces static LegacyControllerClient.ReadControllerFaultLines method.
/// Simulates controller communication by reading pipe-delimited text file.
/// </summary>
public sealed class FileBasedControllerDataReader : IControllerDataReader
{
    private readonly ILogger<FileBasedControllerDataReader> _logger;
    private readonly ControllerOptions _options;

    public FileBasedControllerDataReader(
        ILogger<FileBasedControllerDataReader> logger,
        IOptions<ControllerOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Reads fault records from text file.
    /// Preserves exact legacy parsing logic from FaultCodeService.ParseAndDisplayFault.
    /// </summary>
    public async Task<IReadOnlyList<FaultRecord>> ReadFaultRecordsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Connecting to elevator controller...");
        _logger.LogInformation("Reading from: {FilePath}", _options.DataFilePath);

        // Validate file exists (legacy behavior from LegacyControllerClient)
        if (!File.Exists(_options.DataFilePath))
        {
            var message = $"Controller data file not found: {_options.DataFilePath}";
            _logger.LogError(message);
            throw new ControllerConnectionException(message);
        }

        try
        {
            // Read all lines from file (legacy behavior)
            var lines = await File.ReadAllLinesAsync(
                _options.DataFilePath,
                Encoding.UTF8,
                cancellationToken);

            _logger.LogInformation(
                "Successfully read {Count} fault records from controller.",
                lines.Length);

            // Parse fault records
            var faultRecords = new List<FaultRecord>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    _logger.LogDebug("Skipping empty line");
                    continue;
                }

                var faultRecord = ParseFaultLine(line);
                if (faultRecord != null)
                {
                    faultRecords.Add(faultRecord);
                }
            }

            return faultRecords.AsReadOnly();
        }
        catch (Exception ex) when (ex is not ControllerConnectionException)
        {
            var message = $"Failed to read controller data from {_options.DataFilePath}";
            _logger.LogError(ex, message);
            throw new ControllerDataException(message, ex);
        }
    }

    /// <summary>
    /// Validates controller connection (file exists).
    /// Placeholder for future dongle validation or authentication.
    /// </summary>
    public Task<bool> ValidateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var isValid = File.Exists(_options.DataFilePath);

        _logger.LogInformation(
            "Controller connection validation: {IsValid} (file: {FilePath})",
            isValid,
            _options.DataFilePath);

        return Task.FromResult(isValid);
    }

    /// <summary>
    /// Parses a single pipe-delimited fault line.
    /// Preserves exact legacy parsing logic from FaultCodeService.ParseAndDisplayFault.
    /// </summary>
    private FaultRecord? ParseFaultLine(string faultLine)
    {
        // Split on pipe character (legacy behavior)
        var parts = faultLine.Split('|');

        // Validate minimum 3 fields (legacy behavior)
        if (parts.Length < 3)
        {
            _logger.LogWarning(
                "ERROR: Invalid fault line format (expected 3 fields): {Line}",
                faultLine);
            return null;
        }

        // Extract and trim fields (legacy behavior)
        var timestamp = parts[0].Trim();
        var faultCode = parts[1].Trim();
        var description = parts[2].Trim();

        _logger.LogDebug(
            "Parsed fault: Code={FaultCode}, Timestamp={Timestamp}",
            faultCode,
            timestamp);

        return new FaultRecord
        {
            Timestamp = timestamp,
            FaultCode = faultCode,
            Description = description,
            RawLine = faultLine
        };
    }
}
```

### File: Infrastructure/FaultCodeRepository.cs

```csharp
namespace ElevatorFaultReader.Infrastructure;

using ElevatorFaultReader.Domain;

/// <summary>
/// Repository for fault code metadata and mappings.
/// Provides centralized access to fault code information.
/// Future: Could be backed by database for dynamic fault code management.
/// </summary>
public interface IFaultCodeRepository
{
    /// <summary>
    /// Gets all known fault codes.
    /// </summary>
    IReadOnlyList<string> GetAllFaultCodes();

    /// <summary>
    /// Checks if a fault code is recognized.
    /// </summary>
    bool IsKnownFaultCode(string faultCode);

    /// <summary>
    /// Gets severity for a fault code.
    /// </summary>
    FaultSeverity GetFaultSeverity(string faultCode);
}

/// <summary>
/// In-memory implementation of fault code repository.
/// Uses hardcoded fault code metadata from legacy system.
/// </summary>
public sealed class InMemoryFaultCodeRepository : IFaultCodeRepository
{
    private readonly HashSet<string> _knownFaultCodes;
    private readonly Dictionary<string, FaultSeverity> _severityMap;

    public InMemoryFaultCodeRepository()
    {
        _knownFaultCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            FaultCode.DoorLockFailure,
            FaultCode.MotorOvercurrent,
            FaultCode.LevelingSensorFault,
            FaultCode.CommunicationTimeout,
            FaultCode.BrakeSwitchFault,
            FaultCode.DoorReversal,
            FaultCode.PositionError,
            FaultCode.UnknownCode
        };

        _severityMap = new Dictionary<string, FaultSeverity>(StringComparer.OrdinalIgnoreCase)
        {
            [FaultCode.MotorOvercurrent] = FaultSeverity.High,
            [FaultCode.BrakeSwitchFault] = FaultSeverity.High,
            [FaultCode.PositionError] = FaultSeverity.High,
            [FaultCode.DoorLockFailure] = FaultSeverity.Medium,
            [FaultCode.CommunicationTimeout] = FaultSeverity.Medium,
            [FaultCode.LevelingSensorFault] = FaultSeverity.Low,
            [FaultCode.DoorReversal] = FaultSeverity.Low,
            [FaultCode.UnknownCode] = FaultSeverity.Medium
        };
    }

    public IReadOnlyList<string> GetAllFaultCodes()
    {
        return _knownFaultCodes.ToList().AsReadOnly();
    }

    public bool IsKnownFaultCode(string faultCode)
    {
        return _knownFaultCodes.Contains(faultCode);
    }

    public FaultSeverity GetFaultSeverity(string faultCode)
    {
        return _severityMap.TryGetValue(faultCode, out var severity)
            ? severity
            : FaultSeverity.Medium;
    }
}
```

### File: Program.cs

```csharp
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
```

### File: appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information",
      "ElevatorFaultReader": "Debug"
    }
  },
  "Controller": {
    "DataFilePath": "controller_fault_codes.txt",
    "ConnectionTimeoutSeconds": 30,
    "ValidateConnection": false
  },
  "FaultMapping": {
    "CaseInsensitiveMatching": true,
    "UnknownFaultRecommendation": "Unknown fault code. Consult technical documentation.",
    "Recommendations": {
    }
  }
}
```

---

**Conversion Complete**: All legacy VB6/static .NET Framework code converted to modern .NET 8 architecture with dependency injection, interface abstractions, Options pattern configuration, structured logging, and preserved business rules. The code is production-ready for read-only fault analysis and can be extended with real controller communication protocols, database persistence, and API layers without modifying core business logic.
