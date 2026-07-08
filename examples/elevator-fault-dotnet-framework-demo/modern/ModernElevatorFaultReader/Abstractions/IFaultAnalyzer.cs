namespace ElevatorFaultReader.Abstractions;

using ElevatorFaultReader.Domain;

/// <summary>
/// Abstraction for fault analysis and recommendation logic.
/// Replaces static FaultCodeService with testable interface.
/// </summary>
public interface IFaultAnalyzer
{
    /// <summary>
    /// Analyzes a fault record and returns technician recommendation.
    /// Implements case-insensitive fault code matching (VB6 legacy behavior).
    /// </summary>
    /// <param name="faultRecord">Fault record to analyze.</param>
    /// <returns>Fault recommendation with diagnostic guidance.</returns>
    FaultRecommendation AnalyzeFault(FaultRecord faultRecord);

    /// <summary>
    /// Analyzes multiple fault records and returns recommendations.
    /// </summary>
    /// <param name="faultRecords">Collection of fault records to analyze.</param>
    /// <returns>Collection of fault recommendations.</returns>
    IReadOnlyList<FaultRecommendation> AnalyzeFaults(IEnumerable<FaultRecord> faultRecords);
}