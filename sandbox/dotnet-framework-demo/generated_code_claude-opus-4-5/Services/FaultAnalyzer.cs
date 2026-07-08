using Microsoft.Extensions.Logging;
using ModernElevatorFaultReader.Abstractions;
using ModernElevatorFaultReader.Domain;

namespace ModernElevatorFaultReader.Services;

/// <summary>
/// Default implementation of fault analysis business logic.
/// Replaces legacy FaultCodeService static methods with instance-based service.
/// 
/// BUSINESS RULES PRESERVED:
/// - All 8 fault code mappings from legacy code
/// - Case-insensitive matching (VB6 behavior restored)
/// - Unknown fault code fallback message
/// - Severity levels added for future prioritization
/// </summary>
public class FaultAnalyzer : IFaultAnalyzer
{
    private readonly ILogger<FaultAnalyzer> _logger;
    private readonly IReadOnlyDictionary<string, (string Recommendation, FaultSeverity Severity)> _faultCodeMappings;

    public FaultAnalyzer(ILogger<FaultAnalyzer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _faultCodeMappings = InitializeFaultCodeMappings();
    }

    /// <summary>
    /// Initializes fault code to recommendation mappings.
    /// Uses dictionary for O(1) lookup instead of legacy if/else chain.
    /// All keys are uppercase for case-insensitive matching.
    /// </summary>
    private static IReadOnlyDictionary<string, (string Recommendation, FaultSeverity Severity)> InitializeFaultCodeMappings()
    {
        return new Dictionary<string, (string, FaultSeverity)>(StringComparer.OrdinalIgnoreCase)
        {
            ["DOOR_LOCK_FAILURE"] = (
                "Inspect door lock mechanism and wiring. Check for mechanical obstruction.",
                FaultSeverity.Medium
            ),
            ["MOTOR_OVERCURRENT"] = (
                "Check motor windings and bearings. Verify load conditions. Inspect motor contactor.",
                FaultSeverity.High
            ),
            ["LEVELING_SENSOR_FAULT"] = (
                "Clean leveling sensors. Check sensor alignment and wiring connections.",
                FaultSeverity.Medium
            ),
            ["COMMUNICATION_TIMEOUT"] = (
                "Verify controller communication cable connections. Check for electromagnetic interference.",
                FaultSeverity.Medium
            ),
            ["BRAKE_SWITCH_FAULT"] = (
                "Inspect brake switch operation. Verify brake coil voltage and mechanical linkage.",
                FaultSeverity.High
            ),
            ["DOOR_REVERSAL"] = (
                "Check door reversal sensor and safety edge. Verify door track alignment.",
                FaultSeverity.Low
            ),
            ["POSITION_ERROR"] = (
                "Inspect position encoder and mounting. Check for mechanical wear or misalignment.",
                FaultSeverity.High
            ),
            ["UNKNOWN_CODE"] = (
                "Refer to controller technical manual. Contact manufacturer support if needed.",
                FaultSeverity.Low
            )
        };
    }

    public FaultRecommendation AnalyzeFault(FaultRecord faultRecord)
    {
        if (faultRecord == null)
        {
            throw new ArgumentNullException(nameof(faultRecord));
        }

        // Handle invalid/malformed fault records
        if (!faultRecord.IsValid)
        {
            _logger.LogWarning("Analyzing invalid fault record: {ValidationError}", faultRecord.ValidationError);
            return FaultRecommendation.Create(
                "PARSE_ERROR",
                $"ERROR: {faultRecord.ValidationError}",
                FaultSeverity.Low
            );
        }

        // Case-insensitive lookup (VB6 behavior restored)
        if (_faultCodeMappings.TryGetValue(faultRecord.FaultCode, out var mapping))
        {
            _logger.LogDebug("Fault code {FaultCode} recognized with severity {Severity}", 
                faultRecord.FaultCode, mapping.Severity);

            return FaultRecommendation.Create(
                faultRecord.FaultCode,
                mapping.Recommendation,
                mapping.Severity
            );
        }

        // Unknown fault code fallback (preserves legacy behavior)
        _logger.LogWarning("Unknown fault code encountered: {FaultCode}", faultRecord.FaultCode);
        return FaultRecommendation.CreateUnknown(faultRecord.FaultCode);
    }

    public IReadOnlyList<string> GetRecognizedFaultCodes()
    {
        return _faultCodeMappings.Keys.ToList();
    }
}