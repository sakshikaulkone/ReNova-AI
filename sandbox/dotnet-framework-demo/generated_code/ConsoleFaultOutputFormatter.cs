using Microsoft.Extensions.Logging;
using ModernElevatorFaultReader.Abstractions;
using ModernElevatorFaultReader.Models;

namespace ModernElevatorFaultReader.Services;

/// <summary>
/// Formats fault analysis results for console output.
/// Preserves legacy console output format exactly.
/// </summary>
public sealed class ConsoleFaultOutputFormatter : IFaultOutputFormatter
{
    private readonly ILogger<ConsoleFaultOutputFormatter> _logger;
    private const string Separator = "--------------------------------------------------";

    public ConsoleFaultOutputFormatter(ILogger<ConsoleFaultOutputFormatter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void OutputResult(FaultAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        // Preserve legacy console output format exactly
        Console.WriteLine(Separator);
        Console.WriteLine($"Fault Code: {result.FaultRecord.FaultCode}");
        Console.WriteLine($"Timestamp: {result.FaultRecord.Timestamp}");
        Console.WriteLine($"Description: {result.FaultRecord.Description}");
        Console.WriteLine($"Recommendation: {result.Recommendation}");
        Console.WriteLine(Separator);
        Console.WriteLine();

        _logger.LogInformation(
            "Displayed fault: {FaultCode} at {Timestamp}",
            result.FaultRecord.FaultCode,
            result.FaultRecord.Timestamp);
    }

    /// <inheritdoc/>
    public void OutputError(string faultLine, string errorMessage)
    {
        // Preserve legacy error output format
        Console.WriteLine($"ERROR: {errorMessage}");
        Console.WriteLine($"Line: {faultLine}");
        Console.WriteLine();

        _logger.LogError("Parse error: {ErrorMessage}. Line: {FaultLine}", errorMessage, faultLine);
    }

    /// <inheritdoc/>
    public void OutputStartup(string message)
    {
        Console.WriteLine(message);
        _logger.LogInformation("Startup: {Message}", message);
    }

    /// <inheritdoc/>
    public void OutputCompletion(string message)
    {
        Console.WriteLine(message);
        _logger.LogInformation("Completion: {Message}", message);
    }
}