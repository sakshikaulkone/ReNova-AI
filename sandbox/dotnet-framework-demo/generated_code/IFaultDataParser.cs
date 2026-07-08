namespace ModernElevatorFaultReader.Abstractions;

/// <summary>
/// Abstraction for parsing raw fault data into structured records.
/// Enables testing and alternative parsing implementations.
/// </summary>
public interface IFaultDataParser
{
    /// <summary>
    /// Parses a raw fault line into a structured fault record.
    /// </summary>
    /// <param name="faultLine">Raw fault line from controller (pipe-delimited format)</param>
    /// <returns>Parsed fault record, or null if parsing fails</returns>
    FaultRecord? ParseFaultLine(string faultLine);

    /// <summary>
    /// Validates that a fault line has the expected format.
    /// </summary>
    /// <param name="faultLine">Raw fault line to validate</param>
    /// <returns>True if format is valid, false otherwise</returns>
    bool IsValidFormat(string faultLine);
}