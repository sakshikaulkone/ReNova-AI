using ModernElevatorFaultReader.Domain;

namespace ModernElevatorFaultReader.Presentation;

/// <summary>
/// Console-based presentation layer for fault data and recommendations.
/// Separates output formatting from business logic (replaces embedded Console.WriteLine calls).
/// Preserves legacy console output format for compatibility.
/// 
/// Future enhancement: Could implement IFaultPresenter interface to support
/// alternative output formats (JSON, XML, file logging, etc.).
/// </summary>
public class ConsoleFaultPresenter
{
    private const string SeparatorLine = "--------------------------------------------------";
    private const string HeaderLine = "==========================================";

    /// <summary>
    /// Displays application header (preserves legacy format).
    /// </summary>
    public void DisplayHeader()
    {
        Console.WriteLine(HeaderLine);
        Console.WriteLine("Elevator Fault Reader - Modern .NET 8");
        Console.WriteLine(HeaderLine);
        Console.WriteLine();
    }

    /// <summary>
    /// Displays footer message (preserves legacy format).
    /// </summary>
    public void DisplayFooter()
    {
        Console.WriteLine(HeaderLine);
        Console.WriteLine("Fault processing complete.");
        Console.WriteLine(HeaderLine);
    }

    /// <summary>
    /// Displays count of fault records being processed.
    /// </summary>
    public void DisplayRecordCount(int count)
    {
        Console.WriteLine($"Processing {count} fault records...");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays a fault record with its recommendation.
    /// Preserves legacy output format with severity indicator added.
    /// </summary>
    public void DisplayFaultWithRecommendation(FaultRecord faultRecord, FaultRecommendation recommendation)
    {
        Console.WriteLine(SeparatorLine);

        // Display severity indicator for high-priority faults (new feature)
        if (recommendation.Severity == FaultSeverity.High)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[HIGH SEVERITY]");
            Console.ResetColor();
        }

        Console.WriteLine($"Fault Code: {faultRecord.FaultCode}");
        Console.WriteLine($"Timestamp: {faultRecord.Timestamp}");
        Console.WriteLine($"Description: {faultRecord.Description}");
        Console.WriteLine($"Recommendation: {recommendation.RecommendationText}");

        Console.WriteLine(SeparatorLine);
        Console.WriteLine();
    }

    /// <summary>
    /// Displays an error message with red text.
    /// </summary>
    public void DisplayError(string errorMessage)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERROR: {errorMessage}");
        Console.ResetColor();
        Console.WriteLine();
    }
}