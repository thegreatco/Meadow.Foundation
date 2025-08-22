using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Ili9341 displayRenderer.
/// </summary>
public class Ili9341Simulated : SimulatedDisplayBase
{
    /// <summary>
    /// Create a new simulated Ili9341 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotationType"></param>
    /// <param name="colorMode"></param>
    public Ili9341Simulated(IResizablePixelDisplay displayRenderer,
        RotationType rotationType = RotationType._270Degrees,
        ColorMode colorMode = ColorMode.Format12bppRgb444)
        : base(displayRenderer, 240, 320, rotationType, colorMode)
    { }
}