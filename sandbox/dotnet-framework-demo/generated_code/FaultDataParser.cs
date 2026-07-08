using Microsoft.Extensions.Logging;
using ModernElevatorFaultReader.Abstractions;
using ModernElevatorFaultReader.Models;

namespace ModernElevatorFaultReader.Services;

/// <summary>
/// Parses pipe-delimited fault data into structured records.
/// Preserves legacy parsing logic: split on pipe, validate 3 fields, trim whitespace.
/// </summary>
public sealed class FaultDataParser : IFaultDataParser
{
    private readonly ILogger<FaultDataParser> _logger;
    private const char Delimiter = '|';
    private const int ExpectedFieldCount = 3;

    public FaultDataParser(ILogger<FaultDataParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public FaultRecord? ParseFaultLine(string faultLine)
    {
        if (string.IsNullOrWhiteSpace(faultLine))
        {
            _logger.LogWarning("Received null or empty fault line");
            return null;
        }

        // Split on pipe character (legacy behavior)
        string[] parts = faultLine.Split(Delimiter);

        // Validate field count (legacy behavior: minimum 3 fields)
        if (parts.Length < ExpectedFieldCount)
        {
            _logger.LogWarning(
                "Invalid fault line format: expected {ExpectedFields} fields, got {ActualFields}. Line: {FaultLine}",
                ExpectedFieldCount,
                parts.Length,
                faultLine);
            return null;
        }

        // Extract and trim fields (legacy behavior: Trim() on all fields)
        string timestamp = parts[0].Trim();
        string faultCode = parts[1].Trim();
        string description = parts[2].Trim();

        // Create immutable fault record
        return new FaultRecord
        {
            Timestamp = timestamp,
            FaultCode = faultCode,
            Description = description,
            RawLine = faultLine
        };
    }

    /// <inheritdoc/>
    public bool IsValidFormat(string faultLine)
    {
        if (string.IsNullOrWhiteSpace(faultLine))
        {
            return false;
        }

        string[] parts = faultLine.Split(Delimiter);
        return parts.Length >= ExpectedFieldCount;
    }
}