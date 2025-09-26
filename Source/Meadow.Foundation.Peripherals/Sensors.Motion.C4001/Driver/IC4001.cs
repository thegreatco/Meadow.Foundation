using Meadow.Peripherals.Sensors;
using Meadow.Units;
using static Meadow.Foundation.Sensors.Motion.C4001;

namespace Meadow.Foundation.Sensors.Motion;

/// <summary>
/// Represents a C4001 motion sensor, providing access to motion data such as
/// status, target properties, and motion detection state.
/// </summary>
public interface IC4001 : ISensor
{
    /// <summary>
    /// Sets the sensor's operating mode (e.g., normal, low-power, or test mode).
    /// </summary>
    /// <param name="mode">The <see cref="SensorMode"/> to apply to the sensor.</param>
    /// <returns><c>true</c> if the mode was successfully set; otherwise, <c>false</c>.</returns>
    bool SetSensorMode(SensorMode mode);

    /// <summary>
    /// Retrieves the current status of the sensor.
    /// </summary>
    /// <returns>The current <see cref="SensorStatus"/> indicating sensor readiness or errors.</returns>
    SensorStatus GetStatus();

    /// <summary>
    /// Retrieves the ID or number of the current detected target.
    /// </summary>
    /// <returns>A byte representing the target number.</returns>
    byte GetTargetNumber();

    /// <summary>
    /// Gets the speed of the detected target.
    /// </summary>
    /// <returns>A <see cref="Speed"/> value representing the target's speed in meters per second.</returns>
    Speed GetTargetSpeed();

    /// <summary>
    /// Gets the range (distance) to the detected target.
    /// </summary>
    /// <returns>A <see cref="Length"/> value representing the target's range in meters.</returns>
    Length GetTargetRange();

    /// <summary>
    /// Gets the energy level or strength of the detected target signal.
    /// </summary>
    /// <returns>An unsigned integer representing the target's energy level.</returns>
    uint GetTargetEnergy();

    /// <summary>
    /// Indicates whether motion is currently detected by the sensor.
    /// </summary>
    /// <returns><c>true</c> if motion is detected; otherwise, <c>false</c>.</returns>
    bool IsMotionDetected();
}
