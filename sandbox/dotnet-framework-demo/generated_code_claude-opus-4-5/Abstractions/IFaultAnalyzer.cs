using ModernElevatorFaultReader.Domain;

namespace ModernElevatorFaultReader.Abstractions;

/// <summary>
/// Abstraction for analyzing elevator faults and generating technician recommendations.
/// Replaces legacy static FaultCodeService with testable interface.
/// 
/// Encapsulates business rules for fault code interpretation.
/// Enables unit testing with mock implementations.
/// Supports future alternative analyzers (ML-based, rule engine, etc.).
/// </summary>
public interface IFaultAnalyzer
{
    /// <summary>
    /// Analyzes a fault record and returns a technician recommendation.
    /// Implements case-insensitive fault code matching (VB6 behavior).
    /// Returns fallback recommendation for unknown fault codes.
    /// </summary>
    /// <param name="faultRecord">Fault record to analyze</param>
    /// <returns>Technician recommendation with severity level</returns>
    FaultRecommendation AnalyzeFault(FaultRecord faultRecord);

    /// <summary>
    /// Gets all recognized fault codes supported by this analyzer.
    /// Useful for validation and documentation.
    /// </summary>
    IReadOnlyList<string> GetRecognizedFaultCodes();
}