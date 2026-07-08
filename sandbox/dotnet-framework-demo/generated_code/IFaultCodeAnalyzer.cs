namespace ModernElevatorFaultReader.Abstractions;

/// <summary>
/// Abstraction for analyzing elevator fault codes and providing technician recommendations.
/// Separates business logic from data access and presentation concerns.
/// </summary>
public interface IFaultCodeAnalyzer
{
    /// <summary>
    /// Parses a raw fault line and returns structured analysis result.
    /// Handles malformed data gracefully and provides diagnostic information.
    /// </summary>
    /// <param name="faultLine">Raw fault line from controller (pipe-delimited format)</param>
    /// <returns>Structured fault analysis result with recommendation</returns>
    FaultAnalysisResult AnalyzeFaultLine(string faultLine);

    /// <summary>
    /// Gets technician recommendation for a specific fault code.
    /// Uses case-insensitive matching to preserve VB6 legacy behavior.
    /// </summary>
    /// <param name="faultCode">Fault code from controller</param>
    /// <returns>Technician recommendation text</returns>
    string GetTechnicianRecommendation(string faultCode);
}