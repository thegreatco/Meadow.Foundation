using System.Threading.Tasks;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Defines methods for controlling the state of circuits in a scheduling system.
/// </summary>
public interface ICircuitStateController
{
    /// <summary>
    /// Gets the current state of a circuit.
    /// </summary>
    /// <param name="circuitName">The name of the circuit to get the state for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current state of the circuit.</returns>
    ValueTask<bool> GetCircuitState(string circuitName);

    /// <summary>
    /// Sets the state of a circuit.
    /// </summary>
    /// <param name="circuitName">The name of the circuit to set the state for.</param>
    /// <param name="state">The desired state for the circuit.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetCircuitState(string circuitName, bool state);
}