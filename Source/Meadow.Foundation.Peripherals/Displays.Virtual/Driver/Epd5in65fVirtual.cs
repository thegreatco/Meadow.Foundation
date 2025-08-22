using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// A virtual displayRenderer instance of the Epd5in65 epaper displayRenderer.
///
/// TODO: Move to epapers project when finished.
/// </summary>
public class Epd5in65fVirtual : SimulatedDisplayBase
{
    public Epd5in65fVirtual(IResizablePixelDisplay displayRenderer)
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