using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.CurrentLoop;

/// <summary>
/// Abstract base class for current loop sensors that provides common functionality
/// </summary>
public abstract class CurrentLoopSensor
{
    private readonly ICurrentLoopGenerator _transmitter;

    /// <summary>
    /// Initializes a new instance of the CurrentLoopSensor class
    /// </summary>
    /// <param name="transmitter">The current loop transmitter to use for output</param>
    protected CurrentLoopSensor(ICurrentLoopGenerator transmitter)
    {
        _transmitter = transmitter;
    }

    /// <summary>
    /// Sets the output current, ensuring it's within valid range
    /// </summary>
    /// <param name="current">The current to output</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when current is outside 0-20mA range</exception>
    protected Task SetOutputCurrent(Current current)
    {
        if (current.Milliamps < 0 || current.Milliamps > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(current),
                "Current must be between 0 and 20 milliamps");
        }
        return _transmitter.SetOutputCurrent(current);
    }

    /// <summary>
    /// Sets the currently set output current
    /// </summary>
    public Current GetOutputCurrent()
    {
        return _transmitter.GetOutputCurrent();
    }
}
