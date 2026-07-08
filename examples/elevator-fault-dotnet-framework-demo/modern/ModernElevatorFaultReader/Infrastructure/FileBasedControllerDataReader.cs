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