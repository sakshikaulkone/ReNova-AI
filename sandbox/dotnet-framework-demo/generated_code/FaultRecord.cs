namespace ModernElevatorFaultReader.Models;

/// <summary>
/// Immutable record representing a parsed elevator fault.
/// Uses C# 12 record syntax for value-based equality and immutability.
/// </summary>
public sealed record FaultRecord
{
    /// <summary>
    /// Timestamp when the fault occurred (as string from controller).
    /// </summary>
    public required string Timestamp { get; init; }

    /// <summary>
    /// Fault code identifier (case-insensitive).
    /// </summary>
    public required string FaultCode { get; init; }

    /// <summary>
    /// Human-readable fault description from controller.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Original raw fault line for audit/debugging purposes.
    /// </summary>
    public required string RawLine { get; init; }
}