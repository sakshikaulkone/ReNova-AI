namespace ElevatorFaultReader.Models;

/// <summary>
/// Represents the technician recommendation for a specific fault.
/// Immutable record type for business logic output.
/// </summary>
public record FaultRecommendation
{
    /// <summary>
    /// The fault code this recommendation applies to.
    /// </summary>
    public required string FaultCode { get; init; }

    /// <summary>
    /// Technician recommendation text.
    /// Preserves exact text from legacy FaultCodeService.GetTechnicianRecommendation.
    /// </summary>
    public required string RecommendationText { get; init; }

    /// <summary>
    /// Indicates if this is a high-severity fault requiring immediate attention.
    /// Future enhancement - not present in legacy code.
    /// </summary>
    public bool IsHighSeverity { get; init; }

    /// <summary>
    /// Indicates if this fault code was recognized or is unknown.
    /// </summary>
    public bool IsUnknownCode { get; init; }
}