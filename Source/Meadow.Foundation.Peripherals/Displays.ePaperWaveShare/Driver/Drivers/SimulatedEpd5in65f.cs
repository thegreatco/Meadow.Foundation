using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// A virtual displayRenderer instance of the Epd5in65 epaper displayRenderer.
/// </summary>
public class SimulatedEpd5in65f : SimulatedDisplayBase
{
    /// <summary>
    /// Create a new instance of the simulated Epd5in65f displayRenderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    public SimulatedEpd5in65f(IResizablePixelDisplay displayRenderer)
        : base(displayRenderer, 600, 448, RotationType.Normal, ColorMode.Format4bppIndexed)
    {
        SupportedColorModes = ColorMode.Format4bppIndexed;

        if (pixelBuffer is BufferIndexed4 buffer)
        {
            buffer.IndexedColors[0] = Color.Black;
            buffer.IndexedColors[1] = Color.White;
            buffer.IndexedColors[2] = Color.Green;
            buffer.IndexedColors[3] = Color.Blue;
            buffer.IndexedColors[4] = Color.Red;
            buffer.IndexedColors[5] = Color.Yellow;
            buffer.IndexedColors[6] = Color.Orange;
        }
    }
}