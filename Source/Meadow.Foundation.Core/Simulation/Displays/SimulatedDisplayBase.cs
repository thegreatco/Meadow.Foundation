using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Simulated displayRenderer base class, provides a virtual displayRenderer that renders to a real displayRenderer
/// </summary>
public class SimulatedDisplayBase : IPixelDisplay
{
    /// <inheritdoc/>
    public RotationType Rotation { get; }

    /// <inheritdoc/>
    public ColorMode ColorMode { get; }

    /// <inheritdoc/>
    public ColorMode SupportedColorModes { get; protected set; }

    /// <inheritdoc/>
    public int Width { get; }

    /// <inheritdoc/>
    public int Height { get; }

    /// <summary>
    /// The enabled color for the displayRenderer
    /// </summary>
    protected virtual Color EnabledColor => Color.White;

    /// <summary>
    /// The disabled color for the displayRenderer
    /// </summary>
    protected virtual Color DisabledColor => Color.Black;

    /// <summary>
    /// The pixel buffer for the displayRenderer
    /// </summary>  
    protected IPixelBuffer pixelBuffer = default!;

    /// <summary>
    /// The real displayRenderer that renders that virtual displayRenderer is rendered on
    /// </summary>
    protected IPixelDisplay displayRenderer;

    /// <summary>
    /// The base constructor for a virtual displayRenderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="rotationType"></param>
    /// <param name="colorMode"></param>
    protected SimulatedDisplayBase(
        IResizablePixelDisplay displayRenderer,
        int width, int height,
        RotationType rotationType,
        ColorMode colorMode)
    {
        Width = width;
        Height = height;
        Rotation = rotationType;
        ColorMode = colorMode;

        this.displayRenderer = displayRenderer;
        displayRenderer.Resize(width, height, displayRenderer.DisplayScale);

        CreateBuffer(width, height, colorMode);
    }

    void CreateBuffer(int width, int height, ColorMode colorMode)
    {
        pixelBuffer = colorMode switch
        {
            ColorMode.Format1bpp => new Buffer1bpp(width, height),
            ColorMode.Format4bppGray => new BufferGray4(width, height),
            ColorMode.Format4bppIndexed => new BufferIndexed4(width, height),
            ColorMode.Format8bppGray => new BufferGray8(width, height),
            ColorMode.Format8bppRgb332 => new BufferRgb332(width, height),
            ColorMode.Format12bppRgb444 => new BufferRgb444(width, height),
            ColorMode.Format16bppRgb565 => new BufferRgb565(width, height),
            ColorMode.Format18bppRgb666 => new BufferRgb666(width, height),
            ColorMode.Format24bppRgb888 => new BufferRgb888(width, height),
            _ => throw new System.NotSupportedException($"Color mode {colorMode} is not supported."),
        };
    }

    /// <inheritdoc/>
    public IPixelBuffer PixelBuffer => pixelBuffer;

    /// <inheritdoc/>
    public void Clear(bool updateDisplay = false)
    {
        pixelBuffer.Clear();

        if (updateDisplay)
        {
            Show();
        }
    }

    /// <inheritdoc/>
    public void DrawPixel(int x, int y, Color color)
    {
        pixelBuffer.SetPixel(x, y, color);
    }

    /// <inheritdoc/>
    public void DrawPixel(int x, int y, bool enabled)
    {
        pixelBuffer.SetPixel(x, y, enabled ? EnabledColor : DisabledColor);
    }

    /// <inheritdoc/>
    public void Fill(Color fillColor, bool updateDisplay = false)
    {
        pixelBuffer.Fill(fillColor);
    }

    /// <inheritdoc/>
    public void Fill(int x, int y, int width, int height, Color fillColor)
    {
        pixelBuffer.Fill(x, y, width, height, fillColor);
    }

    /// <inheritdoc/>
    public void InvertPixel(int x, int y)
    {
        pixelBuffer.InvertPixel(x, y);
    }

    /// <inheritdoc/>
    public void Show()
    {
        displayRenderer.WriteBuffer(0, 0, pixelBuffer);
        displayRenderer.Show();
    }

    /// <inheritdoc/>
    public void Show(int left, int top, int right, int bottom)
    {
        //ToDo - not in IPixelDisplay interface
        Show();
    }

    /// <inheritdoc/>
    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        pixelBuffer.WriteBuffer(x, y, displayBuffer);
    }
}