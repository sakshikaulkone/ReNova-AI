using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernElevatorFaultReader.Abstractions;
using ModernElevatorFaultReader.Configuration;
using ModernElevatorFaultReader.Models;

namespace ModernElevatorFaultReader.Services;

/// <summary>
/// Analyzes fault records and generates technician recommendations.
/// Uses case-insensitive fault code matching (restores VB6 behavior).
/// Supports externalized fault code mappings via configuration.
/// </summary>
public sealed class FaultAnalyzer : IFaultAnalyzer
{
    private readonly ILogger<FaultAnalyzer> _logger;
    private readonly FaultCodeOptions _options;
    private readonly Dictionary<string, string> _faultCodeMappings;

    public FaultAnalyzer(
        ILogger<FaultAnalyzer> logger,
        IOptions<FaultCodeOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Build case-insensitive dictionary from configuration
        _faultCodeMappings = _options.FaultCodeMappings
            .ToDictionary(
                m => m.FaultCode,
                m => m.Recommendation,
                StringComparer.OrdinalIgnoreCase); // CRITICAL: Case-insensitive (VB6 behavior)

        _logger.LogInformation(
            "Initialized FaultAnalyzer with {MappingCount} fault code mappings",
            _faultCodeMappings.Count);
    }

    /// <inheritdoc/>
    public FaultAnalysisResult AnalyzeFault(FaultRecord faultRecord)
    {
        ArgumentNullException.ThrowIfNull(faultRecord);

        string recommendation = GetRecommendation(faultRecord.FaultCode);
        bool isRecognized = _faultCodeMappings.ContainsKey(faultRecord.FaultCode);

        _logger.LogDebug(
            "Analyzed fault code {FaultCode}: recognized={IsRecognized}",
            faultRecord.FaultCode,
            isRecognized);

        return new FaultAnalysisResult
        {
            FaultRecord = faultRecord,
            Recommendation = recommendation,
            IsRecognizedFaultCode = isRecognized
        };
    }

    /// <inheritdoc/>
    public string GetRecommendation(string faultCode)
    {
        if (string.IsNullOrWhiteSpace(faultCode))
        {
            return _options.UnknownFaultCodeMessage;
        }

        // Case-insensitive lookup (restores VB6 UCase behavior)
        if (_faultCodeMappings.TryGetValue(faultCode, out string? recommendation))
        {
            return recommendation;
        }

        _logger.LogWarning("Unknown fault code: {FaultCode}", faultCode);
        return _options.UnknownFaultCodeMessage;
    }
}