namespace ElevatorFaultReader.Abstractions;

using ElevatorFaultReader.Domain;

/// <summary>
/// Abstraction for reading fault data from elevator controller.
/// Replaces static LegacyControllerClient with testable interface.
/// Implementations: file-based (current), serial port (future), network (future).
/// </summary>
public interface IControllerDataReader
{
    /// <summary>
    /// Reads fault records from the controller data source.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Collection of fault records read from controller.</returns>
    /// <exception cref="ControllerConnectionException">Thrown when controller connection fails.</exception>
    /// <exception cref="ControllerDataException">Thrown when data reading fails.</exception>
    Task<IReadOnlyList<FaultRecord>> ReadFaultRecordsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates controller connection without reading data.
    /// Placeholder for future dongle validation or authentication.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if connection is valid, false otherwise.</returns>
    Task<bool> ValidateConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Exception thrown when controller connection fails.
/// </summary>
public sealed class ControllerConnectionException : Exception
{
    public ControllerConnectionException(string message) : base(message) { }
    public ControllerConnectionException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when controller data reading fails.
/// </summary>
public sealed class ControllerDataException : Exception
{
    public ControllerDataException(string message) : base(message) { }
    public ControllerDataException(string message, Exception innerException) 
        : base(message, innerException) { }
}