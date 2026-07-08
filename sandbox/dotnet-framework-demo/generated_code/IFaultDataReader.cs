namespace ModernElevatorFaultReader.Abstractions;

/// <summary>
/// Abstraction for reading fault data from elevator controllers.
/// Implementations may read from files, serial ports, network protocols, etc.
/// This interface enables testability and future protocol changes without modifying business logic.
/// </summary>
public interface IFaultDataReader
{
    /// <summary>
    /// Reads raw fault data lines from the controller source.
    /// Each line represents one fault record in the controller's native format.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Collection of raw fault data lines</returns>
    Task<IReadOnlyList<string>> ReadFaultLinesAsync(CancellationToken cancellationToken = default);
}