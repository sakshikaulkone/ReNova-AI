namespace ModernElevatorFaultReader.Abstractions;

/// <summary>
/// Abstraction for formatting and outputting fault analysis results.
/// Enables multiple output formats (console, JSON, file, etc.).
/// </summary>
public interface IFaultOutputFormatter
{
    /// <summary>
    /// Outputs a fault analysis result.
    /// </summary>
    /// <param name="result">Analysis result to output</param>
    void OutputResult(FaultAnalysisResult result);

    /// <summary>
    /// Outputs an error message for a malformed fault line.
    /// </summary>
    /// <param name="faultLine">The malformed fault line</param>
    /// <param name="errorMessage">Error description</param>
    void OutputError(string faultLine, string errorMessage);

    /// <summary>
    /// Outputs a startup message.
    /// </summary>
    /// <param name="message">Message to output</param>
    void OutputStartup(string message);

    /// <summary>
    /// Outputs a completion message.
    /// </summary>
    /// <param name="message">Message to output</param>
    void OutputCompletion(string message);
}