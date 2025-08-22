using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Sample named virtual displayRenderer
///
/// TODO: Move this to the TFT Displays project after we're happy with implementation. Only in here right now for
/// convenience as a WiP.
/// </summary>
public class Ili9341Virtual : VirtualDisplayBase
{
    public Ili9341Virtual(IResizablePixelDisplay displayRenderer,
        RotationType rotationType = RotationType._270Degrees,
        ColorMode colorMode = ColorMode.Format12bppRgb444)
        : base(displayRenderer, 240, 320, rotationType, colorMode)
    {
    }
}