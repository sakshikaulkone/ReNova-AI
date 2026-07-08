namespace ModernElevatorFaultReader.Configuration;

/// <summary>
/// Strongly-typed configuration options for controller data source.
/// Replaces legacy hardcoded file path with externalized configuration.
/// Uses .NET Options pattern with IOptions&lt;T&gt;.
/// </summary>
public class ControllerOptions
{
    public const string SectionName = "Controller";

    /// <summary>
    /// Path to controller data file (for text file simulation).
    /// In production, this would be replaced with serial port, network endpoint, etc.
    /// </summary>
    public string DataFilePath { get; set; } = "controller_fault_codes.txt";

    /// <summary>
    /// Field delimiter character for parsing fault records.
    /// Default: pipe character (|) as per legacy format.
    /// </summary>
    public char FieldDelimiter { get; set; } = '|';

    /// <summary>
    /// Expected number of fields in each fault record.
    /// Default: 3 (timestamp, fault_code, description).
    /// </summary>
    public int ExpectedFieldCount { get; set; } = 3;

    /// <summary>
    /// Whether to continue processing if a malformed record is encountered.
    /// Default: true (preserves legacy behavior).
    /// </summary>
    public bool ContinueOnParseError { get; set; } = true;

    /// <summary>
    /// File encoding for reading controller data.
    /// Default: UTF-8.
    /// </summary>
    public string FileEncoding { get; set; } = "UTF-8";

    /// <summary>
    /// Validates configuration options.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DataFilePath))
        {
            throw new InvalidOperationException("Controller:DataFilePath configuration is required");
        }

        if (ExpectedFieldCount < 1)
        {
            throw new InvalidOperationException("Controller:ExpectedFieldCount must be greater than 0");
        }
    }
}