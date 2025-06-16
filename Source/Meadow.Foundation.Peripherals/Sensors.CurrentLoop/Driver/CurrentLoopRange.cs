namespace Meadow.Foundation.Sensors.CurrentLoop;

/// <summary>
/// Specifies the current loop range for industrial sensors
/// </summary>
public enum CurrentLoopRange
{
    /// <summary>
    /// 0-20mA current loop range
    /// </summary>
    Current_0_20,

    /// <summary>
    /// 4-20mA current loop range (most common in industrial applications)
    /// </summary>
    Current_4_20,
}
