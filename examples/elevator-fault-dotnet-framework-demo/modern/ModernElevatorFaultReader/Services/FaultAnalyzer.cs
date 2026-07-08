namespace ElevatorFaultReader.Services;

using ElevatorFaultReader.Abstractions;
using ElevatorFaultReader.Configuration;
using ElevatorFaultReader.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Default implementation of fault analysis service.
/// Replaces static FaultCodeService.GetTechnicianRecommendation method.
/// Implements case-insensitive fault code matching (VB6 legacy behavior).
/// </summary>
public sealed class FaultAnalyzer : IFaultAnalyzer
{
    private readonly ILogger<FaultAnalyzer> _logger;
    private readonly FaultMappingOptions _options;
    private readonly IReadOnlyDictionary<string, string> _faultRecommendations;

    public FaultAnalyzer(
        ILogger<FaultAnalyzer> logger,
        IOptions<FaultMappingOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Build case-insensitive fault recommendation dictionary
        _faultRecommendations = BuildFaultRecommendations();
    }

    /// <summary>
    /// Analyzes a single fault record and returns recommendation.
    /// Preserves exact legacy business logic with case-insensitive matching.
    /// </summary>
    public FaultRecommendation AnalyzeFault(FaultRecord faultRecord)
    {
        if (faultRecord == null)
        {
            throw new ArgumentNullException(nameof(faultRecord));
        }

        _logger.LogDebug(
            "Analyzing fault code: {FaultCode} at {Timestamp}",
            faultRecord.FaultCode,
            faultRecord.Timestamp);

        // Case-insensitive lookup (VB6 legacy behavior)
        var comparisonType = _options.CaseInsensitiveMatching
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        var matchingKey = _faultRecommendations.Keys
            .FirstOrDefault(k => k.Equals(faultRecord.FaultCode, comparisonType));

        string recommendationText;
        FaultSeverity severity;

        if (matchingKey != null)
        {
            recommendationText = _faultRecommendations[matchingKey];
            severity = GetFaultSeverity(matchingKey);

            _logger.LogInformation(
                "Fault code {FaultCode} matched to recommendation (severity: {Severity})",
                faultRecord.FaultCode,
                severity);
        }
        else
        {
            recommendationText = _options.UnknownFaultRecommendation;
            severity = FaultSeverity.Medium;

            _logger.LogWarning(
                "Unknown fault code: {FaultCode}. Using default recommendation.",
                faultRecord.FaultCode);
        }

        return new FaultRecommendation
        {
            FaultCode = faultRecord.FaultCode,
            RecommendationText = recommendationText,
            Severity = severity
        };
    }

    /// <summary>
    /// Analyzes multiple fault records.
    /// </summary>
    public IReadOnlyList<FaultRecommendation> AnalyzeFaults(IEnumerable<FaultRecord> faultRecords)
    {
        if (faultRecords == null)
        {
            throw new ArgumentNullException(nameof(faultRecords));
        }

        return faultRecords
            .Select(AnalyzeFault)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Builds fault recommendation dictionary from configuration or defaults.
    /// Preserves exact legacy fault code mappings from FaultCodeService.cs.
    /// </summary>
    private IReadOnlyDictionary<string, string> BuildFaultRecommendations()
    {
        // Use configuration mappings if provided, otherwise use legacy defaults
        if (_options.Recommendations.Count > 0)
        {
            _logger.LogInformation(
                "Using {Count} fault mappings from configuration",
                _options.Recommendations.Count);

            return _options.Recommendations.AsReadOnly();
        }

        // Legacy default mappings (preserved exactly from FaultCodeService.cs)
        var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [FaultCode.DoorLockFailure] = 
                "Inspect door lock mechanism and wiring. Check for mechanical obstruction.",

            [FaultCode.MotorOvercurrent] = 
                "Check motor windings and bearings. Verify load conditions. Inspect motor contactor.",

            [FaultCode.LevelingSensorFault] = 
                "Clean leveling sensors. Check sensor alignment and wiring connections.",

            [FaultCode.CommunicationTimeout] = 
                "Verify controller communication cable connections. Check for electromagnetic interference.",

            [FaultCode.BrakeSwitchFault] = 
                "Inspect brake switch operation. Verify brake coil voltage and mechanical linkage.",

            [FaultCode.DoorReversal] = 
                "Check door reversal sensor and safety edge. Verify door track alignment.",

            [FaultCode.PositionError] = 
                "Inspect position encoder and mounting. Check for mechanical wear or misalignment.",

            [FaultCode.UnknownCode] = 
                "Refer to controller technical manual. Contact manufacturer support if needed."
        };

        _logger.LogInformation(
            "Using {Count} default fault mappings (legacy behavior)",
            defaults.Count);

        return defaults;
    }

    /// <summary>
    /// Determines fault severity based on fault code.
    /// High-severity faults: MOTOR_OVERCURRENT, BRAKE_SWITCH_FAULT, POSITION_ERROR
    /// </summary>
    private static FaultSeverity GetFaultSeverity(string faultCode)
    {
        return faultCode.ToUpperInvariant() switch
        {
            "MOTOR_OVERCURRENT" => FaultSeverity.High,
            "BRAKE_SWITCH_FAULT" => FaultSeverity.High,
            "POSITION_ERROR" => FaultSeverity.High,
            "DOOR_LOCK_FAILURE" => FaultSeverity.Medium,
            "COMMUNICATION_TIMEOUT" => FaultSeverity.Medium,
            "LEVELING_SENSOR_FAULT" => FaultSeverity.Low,
            "DOOR_REVERSAL" => FaultSeverity.Low,
            _ => FaultSeverity.Medium
        };
    }
}