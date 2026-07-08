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