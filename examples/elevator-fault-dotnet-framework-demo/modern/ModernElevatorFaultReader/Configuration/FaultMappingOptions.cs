namespace ElevatorFaultReader.Configuration;

/// <summary>
/// Configuration options for fault code mappings.
/// Allows externalization of fault recommendations to configuration.
/// </summary>
public sealed class FaultMappingOptions
{
    public const string SectionName = "FaultMapping";

    /// <summary>
    /// Whether to use case-insensitive fault code matching (VB6 legacy behavior).
    /// </summary>
    public bool CaseInsensitiveMatching { get; set; } = true;

    /// <summary>
    /// Default recommendation for unknown fault codes.
    /// </summary>
    public string UnknownFaultRecommendation { get; set; } = 
        "Unknown fault code. Consult technical documentation.";

    /// <summary>
    /// Dictionary of fault code to recommendation mappings.
    /// Can be overridden in appsettings.json for customization.
    /// </summary>
    public Dictionary<string, string> Recommendations { get; set; } = new();
}