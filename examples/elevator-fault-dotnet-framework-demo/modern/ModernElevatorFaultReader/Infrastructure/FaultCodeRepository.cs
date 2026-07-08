namespace ElevatorFaultReader.Infrastructure;

using ElevatorFaultReader.Domain;

/// <summary>
/// Repository for fault code metadata and mappings.
/// Provides centralized access to fault code information.
/// Future: Could be backed by database for dynamic fault code management.
/// </summary>
public interface IFaultCodeRepository
{
    /// <summary>
    /// Gets all known fault codes.
    /// </summary>
    IReadOnlyList<string> GetAllFaultCodes();

    /// <summary>
    /// Checks if a fault code is recognized.
    /// </summary>
    bool IsKnownFaultCode(string faultCode);

    /// <summary>
    /// Gets severity for a fault code.
    /// </summary>
    FaultSeverity GetFaultSeverity(string faultCode);
}

/// <summary>
/// In-memory implementation of fault code repository.
/// Uses hardcoded fault code metadata from legacy system.
/// </summary>
public sealed class InMemoryFaultCodeRepository : IFaultCodeRepository
{
    private readonly HashSet<string> _knownFaultCodes;
    private readonly Dictionary<string, FaultSeverity> _severityMap;

    public InMemoryFaultCodeRepository()
    {
        _knownFaultCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            FaultCode.DoorLockFailure,
            FaultCode.MotorOvercurrent,
            FaultCode.LevelingSensorFault,
            FaultCode.CommunicationTimeout,
            FaultCode.BrakeSwitchFault,
            FaultCode.DoorReversal,
            FaultCode.PositionError,
            FaultCode.UnknownCode
        };

        _severityMap = new Dictionary<string, FaultSeverity>(StringComparer.OrdinalIgnoreCase)
        {
            [FaultCode.MotorOvercurrent] = FaultSeverity.High,
            [FaultCode.BrakeSwitchFault] = FaultSeverity.High,
            [FaultCode.PositionError] = FaultSeverity.High,
            [FaultCode.DoorLockFailure] = FaultSeverity.Medium,
            [FaultCode.CommunicationTimeout] = FaultSeverity.Medium,
            [FaultCode.LevelingSensorFault] = FaultSeverity.Low,
            [FaultCode.DoorReversal] = FaultSeverity.Low,
            [FaultCode.UnknownCode] = FaultSeverity.Medium
        };
    }

    public IReadOnlyList<string> GetAllFaultCodes()
    {
        return _knownFaultCodes.ToList().AsReadOnly();
    }

    public bool IsKnownFaultCode(string faultCode)
    {
        return _knownFaultCodes.Contains(faultCode);
    }

    public FaultSeverity GetFaultSeverity(string faultCode)
    {
        return _severityMap.TryGetValue(faultCode, out var severity)
            ? severity
            : FaultSeverity.Medium;
    }
}