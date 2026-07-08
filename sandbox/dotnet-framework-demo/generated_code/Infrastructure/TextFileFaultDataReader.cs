using ElevatorFaultReader.Abstractions;
using ElevatorFaultReader.Configuration;
using ElevatorFaultReader.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ElevatorFaultReader.Infrastructure;

/// <summary>
/// Reads fault data from a text file (simulates controller communication).
/// Replaces static LegacyControllerClient.ReadControllerFaultLines method.
/// In production, this would be replaced with a real controller communication implementation
/// (serial port, TCP/IP, Modbus, etc.) implementing the same IFaultDataReader interface.
/// </summary>
public class TextFileFaultDataReader : IFaultDataReader
{
    private readonly ControllerOptions _options;
    private readonly ILogger<TextFileFaultDataReader> _logger;

    public TextFileFaultDataReader(
        IOptions<ControllerOptions> options,
        ILogger<TextFileFaultDataReader> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Reads fault records from text file.
    /// PRESERVED: Exact parsing logic from legacy LegacyControllerClient and FaultCodeService.ParseAndDisplayFault.
    /// </summary>
    public async Task<IReadOnlyList<FaultRecord>> ReadFaultRecordsAsync(CancellationToken cancellationToken = default)
    {
        // PRESERVED: Console messages from legacy LegacyControllerClient.ReadControllerFaultLines
        Console.WriteLine("Connecting to elevator controller...");
        Console.WriteLine($"Reading from: {_options.DataFilePath}");

        // Validate file exists
        if (!File.Exists(_options.DataFilePath))
        {
            var errorMessage = $"Controller data file not found: {_options.DataFilePath}";
            _logger.LogError(errorMessage);
            throw new FileNotFoundException(errorMessage, _options.DataFilePath);
        }

        try
        {
            // Read all lines from file
            // NOTE: Using UTF-8 encoding explicitly (legacy code used default encoding)
            var lines = await File.ReadAllLinesAsync(_options.DataFilePath, cancellationToken);

            _logger.LogInformation("Successfully read {LineCount} fault records from controller", lines.Length);
            Console.WriteLine($"Successfully read {lines.Length} fault records from controller.");
            Console.WriteLine();

            // Parse each line into FaultRecord
            var faultRecords = new List<FaultRecord>();

            foreach (var line in lines)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    var faultRecord = ParseFaultLine(line);
                    faultRecords.Add(faultRecord);
                }
                catch (FormatException ex)
                {
                    // PRESERVED: Continue-on-error behavior from legacy FaultCodeService.ParseAndDisplayFault
                    _logger.LogWarning(ex, "Failed to parse fault line: {Line}", line);

                    if (!_options.ContinueOnParseError)
                    {
                        throw;
                    }

                    // PRESERVED: Exact error message from legacy code
                    Console.WriteLine($"ERROR: Invalid fault line format (expected {_options.ExpectedFieldCount} fields)");
                    Console.WriteLine();
                }
            }

            return faultRecords;
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            _logger.LogError(ex, "Error reading fault data from file: {FilePath}", _options.DataFilePath);
            throw new InvalidOperationException($"Failed to read fault data from controller: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tests if the data file exists (simulates controller connectivity test).
    /// </summary>
    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var exists = File.Exists(_options.DataFilePath);
        _logger.LogDebug("Controller connection test: {Result}", exists ? "Success" : "Failed");
        return Task.FromResult(exists);
    }

    /// <summary>
    /// Parses a single fault line into a FaultRecord.
    /// PRESERVED: Exact parsing logic from legacy FaultCodeService.ParseAndDisplayFault.
    /// </summary>
    private FaultRecord ParseFaultLine(string faultLine)
    {
        // PRESERVED: Split on pipe character
        var parts = faultLine.Split(_options.FieldDelimiter);

        // PRESERVED: Validate minimum field count (legacy checked < 3)
        if (parts.Length < _options.ExpectedFieldCount)
        {
            throw new FormatException($"Invalid fault line format. Expected {_options.ExpectedFieldCount} fields, got {parts.Length}");
        }

        // PRESERVED: Extract and trim fields (exact legacy behavior)
        var timestamp = parts[0].Trim();
        var faultCode = parts[1].Trim();
        var description = parts[2].Trim();

        return new FaultRecord
        {
            Timestamp = timestamp,
            FaultCode = faultCode,
            Description = description,
            RawLine = faultLine
        };
    }
}