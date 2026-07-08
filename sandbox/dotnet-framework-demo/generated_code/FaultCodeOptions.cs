using ModernElevatorFaultReader.Models;

namespace ModernElevatorFaultReader.Configuration;

/// <summary>
/// Configuration options for fault code analysis.
/// Bound from appsettings.json using Options pattern.
/// </summary>
public sealed class FaultCodeOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "FaultCodeAnalysis";

    /// <summary>
    /// Fault code to recommendation mappings.
    /// </summary>
    public List<FaultCodeMapping> FaultCodeMappings { get; init; } = new();

    /// <summary>
    /// Message to display for unknown fault codes.
    /// </summary>
    public string UnknownFaultCodeMessage { get; init; } = "Unknown fault code. Consult technical documentation.";
}