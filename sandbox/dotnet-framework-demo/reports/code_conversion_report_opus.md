# Code Conversion Report

## 1. Summary

Converted legacy .NET Framework 4.7.2 static-method console application to modern .NET 8 architecture using dependency injection, interface abstractions, Options pattern configuration, structured logging, and case-insensitive fault code matching while preserving all 8 fault code mappings and read-only diagnostic behavior.

## 2. Generated Files List

- `Program.cs`
- `Domain/FaultRecord.cs`
- `Domain/FaultRecommendation.cs`
- `Configuration/ControllerOptions.cs`
- `Abstractions/IFaultDataReader.cs`
- `Abstractions/IFaultAnalyzer.cs`
- `Services/FaultAnalyzer.cs`
- `Infrastructure/TextFileFaultDataReader.cs`
- `Presentation/ConsoleFaultPresenter.cs`
- `ModernElevatorFaultReader.csproj`
- `appsettings.json`

## 3. Business Rules Preserved

- All 8 fault code to recommendation mappings preserved exactly (DOOR_LOCK_FAILURE, MOTOR_OVERCURRENT, LEVELING_SENSOR_FAULT, COMMUNICATION_TIMEOUT, BRAKE_SWITCH_FAULT, DOOR_REVERSAL, POSITION_ERROR, UNKNOWN_CODE)
- Case-insensitive fault code matching restored from VB6 behavior using `StringComparison.OrdinalIgnoreCase`
- Pipe-delimited parsing with whitespace trimming and 3-field validation
- Continue-on-error behavior for malformed fault lines
- Unknown fault code fallback message preserved

## 4. Generated Code Blocks

### File: Program.cs

```csharp
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
```

### File: Domain/FaultRecord.cs

```csharp
namespace ModernElevatorFaultReader.Domain;

/// <summary>
/// Immutable domain model representing a single elevator fault record.
/// Replaces legacy string array parsing with strongly-typed record.
/// Uses C# 12 primary constructor and init-only properties.
/// </summary>
public record FaultRecord
{
    /// <summary>
    /// Timestamp when the fault occurred (ISO 8601 format from controller).
    /// </summary>
    public required string Timestamp { get; init; }

    /// <summary>
    /// Fault code identifier (e.g., "DOOR_LOCK_FAILURE").
    /// Case-insensitive matching is handled by the analyzer.
    /// </summary>
    public required string FaultCode { get; init; }

    /// <summary>
    /// Human-readable description of the fault from controller.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Raw fault line from controller for audit/logging purposes.
    /// </summary>
    public string? RawLine { get; init; }

    /// <summary>
    /// Indicates whether this fault record was successfully parsed.
    /// False if the raw line was malformed but processing continued.
    /// </summary>
    public bool IsValid { get; init; } = true;

    /// <summary>
    /// Validation error message if IsValid is false.
    /// </summary>
    public string? ValidationError { get; init; }

    /// <summary>
    /// Factory method to create a valid fault record from parsed components.
    /// </summary>
    public static FaultRecord Create(string timestamp, string faultCode, string description, string? rawLine = null)
    {
        return new FaultRecord
        {
            Timestamp = timestamp?.Trim() ?? string.Empty,
            FaultCode = faultCode?.Trim() ?? string.Empty,
            Description = description?.Trim() ?? string.Empty,
            RawLine = rawLine,
            IsValid = true
        };
    }

    /// <summary>
    /// Factory method to create an invalid fault record for malformed data.
    /// Preserves legacy behavior of continuing processing on parse errors.
    /// </summary>
    public static FaultRecord CreateInvalid(string rawLine, string validationError)
    {
        return new FaultRecord
        {
            Timestamp = string.Empty,
            FaultCode = "PARSE_ERROR",
            Description = "Malformed fault line",
            RawLine = rawLine,
            IsValid = false,
            ValidationError = validationError
        };
    }
}
```

### File: Domain/FaultRecommendation.cs

```csharp
namespace ModernElevatorFaultReader.Domain;

/// <summary>
/// Immutable domain model representing a technician recommendation for a fault.
/// Encapsulates the business logic output from fault analysis.
/// </summary>
public record FaultRecommendation
{
    /// <summary>
    /// The fault code this recommendation applies to.
    /// </summary>
    public required string FaultCode { get; init; }

    /// <summary>
    /// Technician recommendation text with troubleshooting steps.
    /// </summary>
    public required string RecommendationText { get; init; }

    /// <summary>
    /// Severity level for prioritization (future enhancement).
    /// High severity: MOTOR_OVERCURRENT, BRAKE_SWITCH_FAULT, POSITION_ERROR
    /// Medium severity: DOOR_LOCK_FAILURE, LEVELING_SENSOR_FAULT, COMMUNICATION_TIMEOUT
    /// Low severity: DOOR_REVERSAL, UNKNOWN_CODE
    /// </summary>
    public FaultSeverity Severity { get; init; } = FaultSeverity.Medium;

    /// <summary>
    /// Indicates whether this fault code was recognized.
    /// False for unknown/unmapped fault codes.
    /// </summary>
    public bool IsRecognized { get; init; } = true;

    /// <summary>
    /// Factory method to create a recommendation for a recognized fault code.
    /// </summary>
    public static FaultRecommendation Create(string faultCode, string recommendationText, FaultSeverity severity)
    {
        return new FaultRecommendation
        {
            FaultCode = faultCode,
            RecommendationText = recommendationText,
            Severity = severity,
            IsRecognized = true
        };
    }

    /// <summary>
    /// Factory method to create a recommendation for an unknown fault code.
    /// Preserves legacy fallback behavior.
    /// </summary>
    public static FaultRecommendation CreateUnknown(string faultCode)
    {
        return new FaultRecommendation
        {
            FaultCode = faultCode,
            RecommendationText = "Unknown fault code. Consult technical documentation.",
            Severity = FaultSeverity.Low,
            IsRecognized = false
        };
    }
}

/// <summary>
/// Fault severity levels for prioritization.
/// Future enhancement: could drive alerting, escalation, or SLA requirements.
/// </summary>
public enum FaultSeverity
{
    Low = 0,
    Medium = 1,
    High = 2
}
```

### File: Configuration/ControllerOptions.cs

```csharp
namespace ModernElevatorFaultReader.Configuration;

/// <summary>
/// Strongly-typed configuration options for controller data source.
/// Replaces legacy hardcoded file path with externalized configuration.
/// Uses .NET Options pattern with IOptions&lt;T&gt;.
/// </summary>
public class ControllerOptions
{
    public const string SectionName = "Controller";

    /// <summary>
    /// Path to controller data file (for text file simulation).
    /// In production, this would be replaced with serial port, network endpoint, etc.
    /// </summary>
    public string DataFilePath { get; set; } = "controller_fault_codes.txt";

    /// <summary>
    /// Field delimiter character for parsing fault records.
    /// Default: pipe character (|) as per legacy format.
    /// </summary>
    public char FieldDelimiter { get; set; } = '|';

    /// <summary>
    /// Expected number of fields in each fault record.
    /// Default: 3 (timestamp, fault_code, description).
    /// </summary>
    public int ExpectedFieldCount { get; set; } = 3;

    /// <summary>
    /// Whether to continue processing if a malformed record is encountered.
    /// Default: true (preserves legacy behavior).
    /// </summary>
    public bool ContinueOnParseError { get; set; } = true;

    /// <summary>
    /// File encoding for reading controller data.
    /// Default: UTF-8.
    /// </summary>
    public string FileEncoding { get; set; } = "UTF-8";

    /// <summary>
    /// Validates configuration options.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DataFilePath))
        {
            throw new InvalidOperationException("Controller:DataFilePath configuration is required");
        }

        if (ExpectedFieldCount < 1)
        {
            throw new InvalidOperationException("Controller:ExpectedFieldCount must be greater than 0");
        }
    }
}
```

### File: Abstractions/IFaultDataReader.cs

```csharp
using ModernElevatorFaultReader.Domain;

namespace ModernElevatorFaultReader.Abstractions;

/// <summary>
/// Abstraction for reading fault data from elevator controllers.
/// Replaces legacy static LegacyControllerClient with testable interface.
/// 
/// Current implementation: Text file simulation
/// Future implementations: Serial port, TCP/IP, Modbus, proprietary protocols
/// 
/// CRITICAL: This interface does NOT implement real controller communication.
/// Real protocol details (handshake, authentication, binary format) are unknown
/// and must be provided by SME review before production implementation.
/// </summary>
public interface IFaultDataReader
{
    /// <summary>
    /// Reads fault records from the controller data source.
    /// Returns a list of parsed fault records, including invalid records if configured
    /// to continue on parse errors.
    /// </summary>
    /// <returns>List of fault records (valid and invalid)</returns>
    /// <exception cref="FileNotFoundException">If data source is not accessible</exception>
    /// <exception cref="IOException">If data source read fails</exception>
    Task<IReadOnlyList<FaultRecord>> ReadFaultRecordsAsync();

    /// <summary>
    /// Tests connectivity to the controller data source.
    /// Returns true if the data source is accessible, false otherwise.
    /// </summary>
    Task<bool> TestConnectionAsync();
}
```

### File: Abstractions/IFaultAnalyzer.cs

```csharp
using ModernElevatorFaultReader.Domain;

namespace ModernElevatorFaultReader.Abstractions;

/// <summary>
/// Abstraction for analyzing elevator faults and generating technician recommendations.
/// Replaces legacy static FaultCodeService with testable interface.
/// 
/// Encapsulates business rules for fault code interpretation.
/// Enables unit testing with mock implementations.
/// Supports future alternative analyzers (ML-based, rule engine, etc.).
/// </summary>
public interface IFaultAnalyzer
{
    /// <summary>
    /// Analyzes a fault record and returns a technician recommendation.
    /// Implements case-insensitive fault code matching (VB6 behavior).
    /// Returns fallback recommendation for unknown fault codes.
    /// </summary>
    /// <param name="faultRecord">Fault record to analyze</param>
    /// <returns>Technician recommendation with severity level</returns>
    FaultRecommendation AnalyzeFault(FaultRecord faultRecord);

    /// <summary>
    /// Gets all recognized fault codes supported by this analyzer.
    /// Useful for validation and documentation.
    /// </summary>
    IReadOnlyList<string> GetRecognizedFaultCodes();
}
```

### File: Services/FaultAnalyzer.cs

```csharp
using Microsoft.Extensions.Logging;
using ModernElevatorFaultReader.Abstractions;
using ModernElevatorFaultReader.Domain;

namespace ModernElevatorFaultReader.Services;

/// <summary>
/// Default implementation of fault analysis business logic.
/// Replaces legacy FaultCodeService static methods with instance-based service.
/// 
/// BUSINESS RULES PRESERVED:
/// - All 8 fault code mappings from legacy code
/// - Case-insensitive matching (VB6 behavior restored)
/// - Unknown fault code fallback message
/// - Severity levels added for future prioritization
/// </summary>
public class FaultAnalyzer : IFaultAnalyzer
{
    private readonly ILogger<FaultAnalyzer> _logger;
    private readonly IReadOnlyDictionary<string, (string Recommendation, FaultSeverity Severity)> _faultCodeMappings;

    public FaultAnalyzer(ILogger<FaultAnalyzer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _faultCodeMappings = InitializeFaultCodeMappings();
    }

    /// <summary>
    /// Initializes fault code to recommendation mappings.
    /// Uses dictionary for O(1) lookup instead of legacy if/else chain.
    /// All keys are uppercase for case-insensitive matching.
    /// </summary>
    private static IReadOnlyDictionary<string, (string Recommendation, FaultSeverity Severity)> InitializeFaultCodeMappings()
    {
        return new Dictionary<string, (string, FaultSeverity)>(StringComparer.OrdinalIgnoreCase)
        {
            ["DOOR_LOCK_FAILURE"] = (
                "Inspect door lock mechanism and wiring. Check for mechanical obstruction.",
                FaultSeverity.Medium
            ),
            ["MOTOR_OVERCURRENT"] = (
                "Check motor windings and bearings. Verify load conditions. Inspect motor contactor.",
                FaultSeverity.High
            ),
            ["LEVELING_SENSOR_FAULT"] = (
                "Clean leveling sensors. Check sensor alignment and wiring connections.",
                FaultSeverity.Medium
            ),
            ["COMMUNICATION_TIMEOUT"] = (
                "Verify controller communication cable connections. Check for electromagnetic interference.",
                FaultSeverity.Medium
            ),
            ["BRAKE_SWITCH_FAULT"] = (
                "Inspect brake switch operation. Verify brake coil voltage and mechanical linkage.",
                FaultSeverity.High
            ),
            ["DOOR_REVERSAL"] = (
                "Check door reversal sensor and safety edge. Verify door track alignment.",
                FaultSeverity.Low
            ),
            ["POSITION_ERROR"] = (
                "Inspect position encoder and mounting. Check for mechanical wear or misalignment.",
                FaultSeverity.High
            ),
            ["UNKNOWN_CODE"] = (
                "Refer to controller technical manual. Contact manufacturer support if needed.",
                FaultSeverity.Low
            )
        };
    }

    public FaultRecommendation AnalyzeFault(FaultRecord faultRecord)
    {
        if (faultRecord == null)
        {
            throw new ArgumentNullException(nameof(faultRecord));
        }

        // Handle invalid/malformed fault records
        if (!faultRecord.IsValid)
        {
            _logger.LogWarning("Analyzing invalid fault record: {ValidationError}", faultRecord.ValidationError);
            return FaultRecommendation.Create(
                "PARSE_ERROR",
                $"ERROR: {faultRecord.ValidationError}",
                FaultSeverity.Low
            );
        }

        // Case-insensitive lookup (VB6 behavior restored)
        if (_faultCodeMappings.TryGetValue(faultRecord.FaultCode, out var mapping))
        {
            _logger.LogDebug("Fault code {FaultCode} recognized with severity {Severity}", 
                faultRecord.FaultCode, mapping.Severity);

            return FaultRecommendation.Create(
                faultRecord.FaultCode,
                mapping.Recommendation,
                mapping.Severity
            );
        }

        // Unknown fault code fallback (preserves legacy behavior)
        _logger.LogWarning("Unknown fault code encountered: {FaultCode}", faultRecord.FaultCode);
        return FaultRecommendation.CreateUnknown(faultRecord.FaultCode);
    }

    public IReadOnlyList<string> GetRecognizedFaultCodes()
    {
        return _faultCodeMappings.Keys.ToList();
    }
}
```

### File: Infrastructure/TextFileFaultDataReader.cs

```csharp
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernElevatorFaultReader.Abstractions;
using ModernElevatorFaultReader.Configuration;
using ModernElevatorFaultReader.Domain;

namespace ModernElevatorFaultReader.Infrastructure;

/// <summary>
/// Text file-based implementation of IFaultDataReader.
/// Simulates controller communication by reading pipe-delimited text file.
/// Replaces legacy LegacyControllerClient static class.
/// 
/// IMPORTANT: This is a simulation adapter. Real controller communication
/// would require a different implementation based on actual protocol
/// (serial port, TCP/IP, Modbus, proprietary protocol).
/// 
/// PRESERVES LEGACY BEHAVIOR:
/// - Pipe-delimited parsing with 3-field validation
/// - Whitespace trimming on all fields
/// - Continue-on-error for malformed lines
/// - File path from configuration instead of hardcoded
/// </summary>
public class TextFileFaultDataReader : IFaultDataReader
{
    private readonly ControllerOptions _options;
    private readonly ILogger<TextFileFaultDataReader> _logger;

    public TextFileFaultDataReader(
        IOptions<ControllerOptions> options,
        ILogger<TextFileFaultDataReader> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _options.Validate();
    }

    public async Task<IReadOnlyList<FaultRecord>> ReadFaultRecordsAsync()
    {
        _logger.LogInformation("Connecting to elevator controller (text file simulation)");
        _logger.LogInformation("Reading from: {FilePath}", _options.DataFilePath);

        // Check file existence
        if (!File.Exists(_options.DataFilePath))
        {
            _logger.LogError("Controller data file not found: {FilePath}", _options.DataFilePath);
            throw new FileNotFoundException(
                $"Controller data file not found: {_options.DataFilePath}",
                _options.DataFilePath
            );
        }

        // Read all lines from file with configured encoding
        var encoding = Encoding.GetEncoding(_options.FileEncoding);
        var lines = await File.ReadAllLinesAsync(_options.DataFilePath, encoding);

        _logger.LogInformation("Successfully read {Count} fault records from controller", lines.Length);

        // Parse each line into FaultRecord
        var faultRecords = new List<FaultRecord>();
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var faultRecord = ParseFaultLine(line, i + 1);
            faultRecords.Add(faultRecord);

            if (!faultRecord.IsValid && !_options.ContinueOnParseError)
            {
                _logger.LogError("Parse error on line {LineNumber}, stopping processing", i + 1);
                throw new InvalidOperationException(
                    $"Parse error on line {i + 1}: {faultRecord.ValidationError}"
                );
            }
        }

        return faultRecords;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            return await Task.Run(() => File.Exists(_options.DataFilePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing controller connection");
            return false;
        }
    }

    /// <summary>
    /// Parses a single pipe-delimited fault line into a FaultRecord.
    /// Preserves legacy parsing logic with 3-field validation and trimming.
    /// </summary>
    private FaultRecord ParseFaultLine(string line, int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            _logger.LogWarning("Empty line encountered at line {LineNumber}", lineNumber);
            return FaultRecord.CreateInvalid(line, "Empty or whitespace-only line");
        }

        // Split on configured delimiter (default: pipe)
        var parts = line.Split(_options.FieldDelimiter);

        // Validate field count (preserves legacy behavior)
        if (parts.Length < _options.ExpectedFieldCount)
        {
            var error = $"Invalid fault line format (expected {_options.ExpectedFieldCount} fields, got {parts.Length})";
            _logger.LogWarning("Parse error at line {LineNumber}: {Error}", lineNumber, error);
            return FaultRecord.CreateInvalid(line, error);
        }

        // Extract and trim fields (preserves legacy behavior)
        var timestamp = parts[0].Trim();
        var faultCode = parts[1].Trim();
        var description = parts[2].Trim();

        // Validate non-empty fields
        if (string.IsNullOrWhiteSpace(faultCode))
        {
            var error = "Fault code field is empty";
            _logger.LogWarning("Parse error at line {LineNumber}: {Error}", lineNumber, error);
            return FaultRecord.CreateInvalid(line, error);
        }

        return FaultRecord.Create(timestamp, faultCode, description, line);
    }
}
```

### File: Presentation/ConsoleFaultPresenter.cs

```csharp
using ModernElevatorFaultReader.Domain;

namespace ModernElevatorFaultReader.Presentation;

/// <summary>
/// Console-based presentation layer for fault data and recommendations.
/// Separates output formatting from business logic (replaces embedded Console.WriteLine calls).
/// Preserves legacy console output format for compatibility.
/// 
/// Future enhancement: Could implement IFaultPresenter interface to support
/// alternative output formats (JSON, XML, file logging, etc.).
/// </summary>
public class ConsoleFaultPresenter
{
    private const string SeparatorLine = "--------------------------------------------------";
    private const string HeaderLine = "==========================================";

    /// <summary>
    /// Displays application header (preserves legacy format).
    /// </summary>
    public void DisplayHeader()
    {
        Console.WriteLine(HeaderLine);
        Console.WriteLine("Elevator Fault Reader - Modern .NET 8");
        Console.WriteLine(HeaderLine);
        Console.WriteLine();
    }

    /// <summary>
    /// Displays footer message (preserves legacy format).
    /// </summary>
    public void DisplayFooter()
    {
        Console.WriteLine(HeaderLine);
        Console.WriteLine("Fault processing complete.");
        Console.WriteLine(HeaderLine);
    }

    /// <summary>
    /// Displays count of fault records being processed.
    /// </summary>
    public void DisplayRecordCount(int count)
    {
        Console.WriteLine($"Processing {count} fault records...");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays a fault record with its recommendation.
    /// Preserves legacy output format with severity indicator added.
    /// </summary>
    public void DisplayFaultWithRecommendation(FaultRecord faultRecord, FaultRecommendation recommendation)
    {
        Console.WriteLine(SeparatorLine);

        // Display severity indicator for high-priority faults (new feature)
        if (recommendation.Severity == FaultSeverity.High)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[HIGH SEVERITY]");
            Console.ResetColor();
        }

        Console.WriteLine($"Fault Code: {faultRecord.FaultCode}");
        Console.WriteLine($"Timestamp: {faultRecord.Timestamp}");
        Console.WriteLine($"Description: {faultRecord.Description}");
        Console.WriteLine($"Recommendation: {recommendation.RecommendationText}");

        Console.WriteLine(SeparatorLine);
        Console.WriteLine();
    }

    /// <summary>
    /// Displays an error message with red text.
    /// </summary>
    public void DisplayError(string errorMessage)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERROR: {errorMessage}");
        Console.ResetColor();
        Console.WriteLine();
    }
}
```

### File: ModernElevatorFaultReader.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>12.0</LangVersion>
    <RootNamespace>ModernElevatorFaultReader</RootNamespace>
    <AssemblyName>ModernElevatorFaultReader</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.CommandLine" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="appsettings.Development.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="controller_fault_codes.txt">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

### File: appsettings.json

```json
{
  "Controller": {
    "DataFilePath": "controller_fault_codes.txt",
    "FieldDelimiter": "|",
    "ExpectedFieldCount": 3,
    "ContinueOnParseError": true,
    "FileEncoding": "UTF-8"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "ModernElevatorFaultReader": "Debug"
    }
  }
}
```
