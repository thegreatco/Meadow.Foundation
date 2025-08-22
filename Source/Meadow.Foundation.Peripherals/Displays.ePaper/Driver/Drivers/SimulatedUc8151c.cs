using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a simulated Uc8151c displayRenderer
/// </summary>
public class SimulatedUc8151c : SimulatedDisplayBase
{
    /// <summary>
    /// The color modes supported by the display
    /// </summary>
    public override ColorMode SupportedColorModes => ColorMode.Format2bppIndexed;

    /// <inheritdoc/>
    protected override Color EnabledColor => Color.Black;

    /// <inheritdoc/>
    protected override Color DisabledColor => Color.White;

    /// <summary>
    /// Create a new simulated SSUc8151cD1681 displayRenderer.
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="rotate"></param>
    public SimulatedUc8151c(IResizablePixelDisplay displayRenderer,
        bool rotate = true)
        : base(displayRenderer, 152, 152, rotate, ColorMode.Format2bppIndexed)
    {
        if (pixelBuffer is BufferIndexed2 buffer)
        {
            buffer.IndexedColors[0] = Color.Black;
            buffer.IndexedColors[1] = Color.White;
            buffer.IndexedColors[2] = Color.Red;
        }
    }
}