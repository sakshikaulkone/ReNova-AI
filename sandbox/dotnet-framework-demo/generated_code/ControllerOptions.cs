namespace ModernElevatorFaultReader.Configuration;

/// <summary>
/// Strongly-typed configuration options for controller data source.
/// Bound from appsettings.json using Options pattern.
/// Replaces hardcoded file path from legacy Program.cs.
/// </summary>
public sealed class ControllerOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Controller";

    /// <summary>
    /// Path to controller data file (for file-based simulation).
    /// In production, this would be replaced with serial port, network endpoint, etc.
    /// </summary>
    public string DataFilePath { get; set; } = "controller_fault_codes.txt";

    /// <summary>
    /// Connection timeout in seconds (reserved for future real controller communication).
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to validate controller connection before reading data (reserved for future use).
    /// </summary>
    public bool ValidateConnection { get; set; } = false;
}