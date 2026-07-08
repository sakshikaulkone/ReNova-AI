namespace ElevatorFaultReader.Domain;

/// <summary>
/// Represents a technician recommendation for a specific fault code.
/// Immutable record containing diagnostic guidance.
/// </summary>
public sealed record FaultRecommendation
{
    /// <summary>
    /// The fault code this recommendation applies to.
    /// </summary>
    public required string FaultCode { get; init; }

    /// <summary>
    /// Technician recommendation text with diagnostic steps.
    /// </summary>
    public required string RecommendationText { get; init; }

    /// <summary>
    /// Severity level for future prioritization (not used in legacy system).
    /// </summary>
    public FaultSeverity Severity { get; init; } = FaultSeverity.Medium;

    /// <summary>
    /// Whether this is a high-priority fault requiring immediate attention.
    /// </summary>
    public bool IsHighPriority => Severity == FaultSeverity.High;
}