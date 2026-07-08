using Microsoft.Extensions.Logging;
using ModernElevatorFaultReader.Abstractions;
using ModernElevatorFaultReader.Models;

namespace ModernElevatorFaultReader.Services;

/// <summary>
/// Implementation of fault code analysis business logic.
/// Preserves exact fault code mappings and recommendations from legacy FaultCodeService.cs.
/// Restores case-insensitive matching behavior from VB6 (fixes C# regression).
/// </summary>
public sealed class FaultCodeAnalyzer : IFaultCodeAnalyzer
{
    private readonly ILogger<FaultCodeAnalyzer> _logger;

    // Fault code to recommendation mapping preserved from legacy code
    private static readonly IReadOnlyDictionary<string, string> FaultRecommendations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["DOOR_LOCK_FAILURE"] = "Inspect door lock mechanism and wiring. Check for mechanical obstruction.",
        ["MOTOR_OVERCURRENT"] = "Check motor windings and bearings. Verify load conditions. Inspect motor contactor.",
        ["LEVELING_SENSOR_FAULT"] = "Clean leveling sensors. Check sensor alignment and wiring connections.",
        ["COMMUNICATION_TIMEOUT"] = "Verify controller communication cable connections. Check for electromagnetic interference.",
        ["BRAKE_SWITCH_FAULT"] = "Inspect brake switch operation. Verify brake coil voltage and mechanical linkage.",
        ["DOOR_REVERSAL"] = "Check door reversal sensor and safety edge. Verify door track alignment.",
        ["POSITION_ERROR"] = "Inspect position encoder and mounting. Check for mechanical wear or misalignment.",
        ["UNKNOWN_CODE"] = "Refer to controller technical manual. Contact manufacturer support if needed."
    };

    private const string DefaultRecommendation = "Unknown fault code. Consult technical documentation.";

    public FaultCodeAnalyzer(ILogger<FaultCodeAnalyzer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public FaultAnalysisResult AnalyzeFaultLine(string faultLine)
    {
        if (string.IsNullOrWhiteSpace(faultLine))
        {
            const string errorMessage = "ERROR: Fault line is null or empty";
            _logger.LogWarning(errorMessage);
            return FaultAnalysisResult.Failure(errorMessage);
        }

        // Parse pipe-delimited format: timestamp|fault_code|description
        string[] parts = faultLine.Split('|');

        // Validate minimum field count (preserves legacy behavior)
        if (parts.Length < 3)
        {
            const string errorMessage = "ERROR: Invalid fault line format (expected 3 fields)";
            _logger.LogWarning("{ErrorMessage}. Line: {FaultLine}", errorMessage, faultLine);
            return FaultAnalysisResult.Failure(errorMessage);
        }

        // Extract and trim fields (preserves legacy trim behavior)
        string timestamp = parts[0].Trim();
        string faultCode = parts[1].Trim();
        string description = parts[2].Trim();

        // Get technician recommendation
        string recommendation = GetTechnicianRecommendation(faultCode);

        // Create structured fault record
        var faultRecord = new FaultRecord
        {
            Timestamp = timestamp,
            FaultCode = faultCode,
            Description = description,
            Recommendation = recommendation
        };

        _logger.LogDebug("Successfully parsed fault: {FaultCode} at {Timestamp}", faultCode, timestamp);

        return FaultAnalysisResult.Success(faultRecord);
    }

    /// <inheritdoc />
    public string GetTechnicianRecommendation(string faultCode)
    {
        if (string.IsNullOrWhiteSpace(faultCode))
        {
            return DefaultRecommendation;
        }

        // Case-insensitive lookup (restores VB6 behavior, fixes C# regression)
        // Dictionary uses StringComparer.OrdinalIgnoreCase for case-insensitive matching
        return FaultRecommendations.TryGetValue(faultCode, out string? recommendation)
            ? recommendation
            : DefaultRecommendation;
    }
}