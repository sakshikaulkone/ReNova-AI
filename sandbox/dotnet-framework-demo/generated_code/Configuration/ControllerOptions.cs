namespace ElevatorFaultReader.Configuration;

/// <summary>
/// Configuration options for controller data source.
/// Replaces hardcoded file path from legacy Program.cs.
/// </summary>
public sealed class ControllerOptions
{
    public const string SectionName = "Controller";

    /// <summary>
    /// Path to controller data file (for file-based simulation).
    /// In production, this would be replaced with serial port or network configuration.
    /// </summary>
    public string DataFilePath { get; set; } = "controller_fault_codes.txt";

    /// <summary>
    /// Connection timeout in seconds (for future real controller communication).
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to validate controller connection before reading data.
    /// Placeholder for future dongle/authentication logic.
    /// </summary>
    public bool ValidateConnection { get; set; } = false;
}