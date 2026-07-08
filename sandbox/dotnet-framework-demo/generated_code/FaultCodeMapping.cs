namespace ModernElevatorFaultReader.Models;

/// <summary>
/// Configuration record for fault code to recommendation mapping.
/// Supports externalized configuration via appsettings.json.
/// </summary>
public sealed record FaultCodeMapping
{
    /// <summary>
    /// Fault code identifier (case-insensitive).
    /// </summary>
    public required string FaultCode { get; init; }

    /// <summary>
    /// Technician recommendation text.
    /// </summary>
    public required string Recommendation { get; init; }
}