using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Gc9a01 displayRenderer
/// </summary>
public class SimulatedGc9a01 : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565;

    /// <summary>
    /// Create a new simulated Gc9a01 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotationType"></param>
    /// <param name="colorMode"></param>
    public SimulatedGc9a01(IResizablePixelDisplay displayRenderer,
        RotationType rotationType = RotationType._270Degrees,
        ColorMode colorMode = ColorMode.Format16bppRgb565)
        : base(displayRenderer, 320, 480, rotationType, colorMode)
    { }
}