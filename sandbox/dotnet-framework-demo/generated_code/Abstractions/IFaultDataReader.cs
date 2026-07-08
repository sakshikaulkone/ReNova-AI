using ElevatorFaultReader.Models;

namespace ElevatorFaultReader.Abstractions;

/// <summary>
/// Abstraction for reading fault data from elevator controllers.
/// Replaces static LegacyControllerClient.ReadControllerFaultLines method.
/// Enables future implementations for real controller protocols (serial, TCP/IP, Modbus, etc.).
/// </summary>
public interface IFaultDataReader
{
    /// <summary>
    /// Reads fault records from the controller.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of parsed fault records.</returns>
    /// <exception cref="InvalidOperationException">Thrown if controller connection fails.</exception>
    /// <exception cref="FormatException">Thrown if fault data format is invalid.</exception>
    Task<IReadOnlyList<FaultRecord>> ReadFaultRecordsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests connectivity to the controller without reading data.
    /// Future enhancement for real controller communication.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if controller is reachable, false otherwise.</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}