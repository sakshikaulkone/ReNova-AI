namespace ModernElevatorFaultReader.Domain;

/// <summary>
/// Immutable domain model representing a technician recommendation for a fault.
/// Encapsulates the business logic output from fault analysis.
/// </summary>
public record FaultRecommendation
{
    /// <summary>
    /// The fault code this recommendation applies to.
    /// </summary>
    public required string FaultCode { get; init; }

    /// <summary>
    /// Technician recommendation text with troubleshooting steps.
    /// </summary>
    public required string RecommendationText { get; init; }

    /// <summary>
    /// Severity level for prioritization (future enhancement).
    /// High severity: MOTOR_OVERCURRENT, BRAKE_SWITCH_FAULT, POSITION_ERROR
    /// Medium severity: DOOR_LOCK_FAILURE, LEVELING_SENSOR_FAULT, COMMUNICATION_TIMEOUT
    /// Low severity: DOOR_REVERSAL, UNKNOWN_CODE
    /// </summary>
    public FaultSeverity Severity { get; init; } = FaultSeverity.Medium;

    /// <summary>
    /// Indicates whether this fault code was recognized.
    /// False for unknown/unmapped fault codes.
    /// </summary>
    public bool IsRecognized { get; init; } = true;

    /// <summary>
    /// Factory method to create a recommendation for a recognized fault code.
    /// </summary>
    public static FaultRecommendation Create(string faultCode, string recommendationText, FaultSeverity severity)
    {
        return new FaultRecommendation
        {
            FaultCode = faultCode,
            RecommendationText = recommendationText,
            Severity = severity,
            IsRecognized = true
        };
    }

    /// <summary>
    /// Factory method to create a recommendation for an unknown fault code.
    /// Preserves legacy fallback behavior.
    /// </summary>
    public static FaultRecommendation CreateUnknown(string faultCode)
    {
        return new FaultRecommendation
        {
            FaultCode = faultCode,
            RecommendationText = "Unknown fault code. Consult technical documentation.",
            Severity = FaultSeverity.Low,
            IsRecognized = false
        };
    }
}

/// <summary>
/// Fault severity levels for prioritization.
/// Future enhancement: could drive alerting, escalation, or SLA requirements.
/// </summary>
public enum FaultSeverity
{
    Low = 0,
    Medium = 1,
    High = 2
}