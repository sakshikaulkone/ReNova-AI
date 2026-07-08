using ModernElevatorFaultReader.Domain;

namespace ModernElevatorFaultReader.Abstractions;

/// <summary>
/// Abstraction for reading fault data from elevator controllers.
/// Replaces legacy static LegacyControllerClient with testable interface.
/// 
/// Current implementation: Text file simulation
/// Future implementations: Serial port, TCP/IP, Modbus, proprietary protocols
/// 
/// CRITICAL: This interface does NOT implement real controller communication.
/// Real protocol details (handshake, authentication, binary format) are unknown
/// and must be provided by SME review before production implementation.
/// </summary>
public interface IFaultDataReader
{
    /// <summary>
    /// Reads fault records from the controller data source.
    /// Returns a list of parsed fault records, including invalid records if configured
    /// to continue on parse errors.
    /// </summary>
    /// <returns>List of fault records (valid and invalid)</returns>
    /// <exception cref="FileNotFoundException">If data source is not accessible</exception>
    /// <exception cref="IOException">If data source read fails</exception>
    Task<IReadOnlyList<FaultRecord>> ReadFaultRecordsAsync();

    /// <summary>
    /// Tests connectivity to the controller data source.
    /// Returns true if the data source is accessible, false otherwise.
    /// </summary>
    Task<bool> TestConnectionAsync();
}