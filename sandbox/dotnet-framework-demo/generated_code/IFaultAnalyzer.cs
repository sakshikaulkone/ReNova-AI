namespace ModernElevatorFaultReader.Abstractions;

/// <summary>
/// Abstraction for analyzing fault records and generating technician recommendations.
/// Enables testing and alternative analysis implementations.
/// </summary>
public interface IFaultAnalyzer
{
    /// <summary>
    /// Analyzes a fault record and generates a recommendation.
    /// </summary>
    /// <param name="faultRecord">Parsed fault record</param>
    /// <returns>Analysis result with recommendation</returns>
    FaultAnalysisResult AnalyzeFault(FaultRecord faultRecord);

    /// <summary>
    /// Gets the recommendation text for a specific fault code.
    /// </summary>
    /// <param name="faultCode">Fault code identifier</param>
    /// <returns>Technician recommendation text</returns>
    string GetRecommendation(string faultCode);
}