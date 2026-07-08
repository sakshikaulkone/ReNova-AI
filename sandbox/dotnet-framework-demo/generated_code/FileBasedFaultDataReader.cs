using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernElevatorFaultReader.Abstractions;
using ModernElevatorFaultReader.Configuration;

namespace ModernElevatorFaultReader.Services;

/// <summary>
/// File-based implementation of IFaultDataReader for development and testing.
/// Reads fault data from a text file to simulate controller output.
/// In production, this would be replaced with SerialPortFaultDataReader, NetworkFaultDataReader, etc.
/// Replaces legacy LegacyControllerClient.cs static class.
/// </summary>
public sealed class FileBasedFaultDataReader : IFaultDataReader
{
    private readonly ControllerOptions _options;
    private readonly ILogger<FileBasedFaultDataReader> _logger;

    public FileBasedFaultDataReader(
        IOptions<ControllerOptions> options,
        ILogger<FileBasedFaultDataReader> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ReadFaultLinesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Connecting to elevator controller...");
        _logger.LogInformation("Reading from: {FilePath}", _options.DataFilePath);

        // Validate file exists
        if (!File.Exists(_options.DataFilePath))
        {
            var errorMessage = $"Controller data file not found: {_options.DataFilePath}";
            _logger.LogError(errorMessage);
            throw new FileNotFoundException(errorMessage, _options.DataFilePath);
        }

        try
        {
            // Read all fault lines asynchronously
            string[] faultLines = await File.ReadAllLinesAsync(_options.DataFilePath, cancellationToken);

            _logger.LogInformation("Successfully read {RecordCount} fault records from controller", faultLines.Length);

            return faultLines;
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            _logger.LogError(ex, "Error reading fault data from file: {FilePath}", _options.DataFilePath);
            throw;
        }
    }
}