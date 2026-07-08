namespace ModernElevatorFaultReader.Models;

/// <summary>
/// Immutable record representing the result of fault analysis.
/// Contains the original fault record plus the generated recommendation.
/// </summary>
public sealed record FaultAnalysisResult
{
    /// <summary>
    /// The original parsed fault record.
    /// </summary>
    public required FaultRecord FaultRecord { get; init; }

    /// <summary>
    /// Technician recommendation based on fault code analysis.
    /// </summary>
    public required string Recommendation { get; init; }

    /// <summary>
    /// Indicates whether the fault code was recognized.
    /// </summary>
    public required bool IsRecognizedFaultCode { get; init; }
}