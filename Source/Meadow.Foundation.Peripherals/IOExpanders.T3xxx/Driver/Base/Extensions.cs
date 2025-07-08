using Meadow.Foundation.IOExpanders;
using Meadow.Hardware;
using System;

namespace Meadow.Devices;

/// <summary>
/// Extension methods for T3 modules
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Creates an IDigitalInputPort using the specified pin.
    /// </summary>
    /// <param name="pin">The pin to use for the input port</param>
    /// <returns>an IDigitalInputPort instance</returns>
    public static IDigitalInputPort CreateDigitalInputPort(this T3xxxPin pin)
    {
        if (pin.Controller is T322ai t322)
        {
            return t322.CreateDigitalInputPort(pin);
        }

        throw new NotSupportedException("pin does not support Digital Input");
    }

    /// <summary>
    /// Creates an ICurrentInputPort using the specified pin.
    /// </summary>
    /// <param name="pin">The pin to use for the input port</param>
    public static ICurrentInputPort CreateCurrentInputPort(this IPin pin)
    {
        if (pin.Controller is T322ai t322)
        {
            return t322.CreateCurrentInputPort(pin).GetAwaiter().GetResult();
        }

        throw new NotSupportedException("pin does not support Digital Input");
    }
}
