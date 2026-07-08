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