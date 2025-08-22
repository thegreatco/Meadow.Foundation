using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Hx8357b displayRenderer
/// </summary>
public class SimulatedHx8357b : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565;

    /// <summary>
    /// Create a new simulated Hx8357b displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotationType"></param>
    /// <param name="colorMode"></param>
    public SimulatedHx8357b(IResizablePixelDisplay displayRenderer,
        RotationType rotationType = RotationType._270Degrees,
        ColorMode colorMode = ColorMode.Format16bppRgb565)
        : base(displayRenderer, 240, 240, rotationType, colorMode)
    { }
}