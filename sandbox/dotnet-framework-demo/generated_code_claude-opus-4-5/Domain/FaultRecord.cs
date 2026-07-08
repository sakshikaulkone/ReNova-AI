namespace ModernElevatorFaultReader.Domain;

/// <summary>
/// Immutable domain model representing a single elevator fault record.
/// Replaces legacy string array parsing with strongly-typed record.
/// Uses C# 12 primary constructor and init-only properties.
/// </summary>
public record FaultRecord
{
    /// <summary>
    /// Timestamp when the fault occurred (ISO 8601 format from controller).
    /// </summary>
    public required string Timestamp { get; init; }

    /// <summary>
    /// Fault code identifier (e.g., "DOOR_LOCK_FAILURE").
    /// Case-insensitive matching is handled by the analyzer.
    /// </summary>
    public required string FaultCode { get; init; }

    /// <summary>
    /// Human-readable description of the fault from controller.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Raw fault line from controller for audit/logging purposes.
    /// </summary>
    public string? RawLine { get; init; }

    /// <summary>
    /// Indicates whether this fault record was successfully parsed.
    /// False if the raw line was malformed but processing continued.
    /// </summary>
    public bool IsValid { get; init; } = true;

    /// <summary>
    /// Validation error message if IsValid is false.
    /// </summary>
    public string? ValidationError { get; init; }

    /// <summary>
    /// Factory method to create a valid fault record from parsed components.
    /// </summary>
    public static FaultRecord Create(string timestamp, string faultCode, string description, string? rawLine = null)
    {
        return new FaultRecord
        {
            Timestamp = timestamp?.Trim() ?? string.Empty,
            FaultCode = faultCode?.Trim() ?? string.Empty,
            Description = description?.Trim() ?? string.Empty,
            RawLine = rawLine,
            IsValid = true
        };
    }

    /// <summary>
    /// Factory method to create an invalid fault record for malformed data.
    /// Preserves legacy behavior of continuing processing on parse errors.
    /// </summary>
    public static FaultRecord CreateInvalid(string rawLine, string validationError)
    {
        return new FaultRecord
        {
            Timestamp = string.Empty,
            FaultCode = "PARSE_ERROR",
            Description = "Malformed fault line",
            RawLine = rawLine,
            IsValid = false,
            ValidationError = validationError
        };
    }
}