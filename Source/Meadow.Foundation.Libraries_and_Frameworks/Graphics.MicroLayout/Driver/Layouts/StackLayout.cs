namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A layout that arranges child elements in a horizontal or vertical stack.
/// </summary>
public class StackLayout : LayoutBase
{
    /// <summary>
    /// Defines the stacking orientation (Vertical or Horizontal).
    /// </summary>
    public enum Orientation
    {
        /// <summary>
        /// Layout controls vertically
        /// </summary>
        Vertical,
        /// <summary>
        /// Layout controls horizontally
        /// </summary>
        Horizontal
    }

    private Orientation _stackOrientation;
    private int _spacing = 2;

    /// <summary>
    /// Gets or sets the stack orientation.
    /// </summary>
    public Orientation StackOrientation
    {
        get => _stackOrientation;
        set
        {
            if (_stackOrientation != value)
            {
                _stackOrientation = value;
                LayoutControls();
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the spacing between child elements.
    /// </summary>
    public int Spacing
    {
        get => _spacing;
        set
        {
            if (_spacing != value)
            {
                _spacing = value;
                LayoutControls();
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Creates a new StackLayout.
    /// </summary>
    public StackLayout()
        : this(0, 0, 0, 0)
    {
    }

    /// <summary>
    /// Creates a new StackLayout.
    /// </summary>
    /// <param name="left">The left position of the layout.</param>
    /// <param name="top">The top position of the layout.</param>
    /// <param name="width">The width of the layout.</param>
    /// <param name="height">The height of the layout.</param>
    /// <param name="orientation">The stacking orientation.</param>
    public StackLayout(int left, int top, int width, int height, Orientation orientation = Orientation.Vertical)
        : base(left, top, width, height)
    {
        _stackOrientation = orientation;

        Controls.ControlAdded += OnControlsChanged;
        Controls.ControlRemoved += OnControlsChanged;
    }

    private void OnControlsChanged(object sender, IControl e)
    {
        LayoutControls();
        Invalidate();
    }

    /// <summary>
    /// Arranges child controls based on the stack orientation and indentation.
    /// </summary>
    public void LayoutControls()
    {
        int offset = 0;

        lock (Controls.SyncRoot)
        {
            foreach (var control in Controls)
            {
                if (!control.IsVisible)
                {
                    continue;
                }

                if (StackOrientation == Orientation.Vertical)
                {
                    control.Left = Width / 2 - control.Width / 2;
                    control.Top = offset;
                    offset += control.Height + Spacing;
                }
                else
                {
                    control.Left = offset;
                    control.Top = Height / 2 - control.Height / 2;
                    offset += control.Width + Spacing;
                }
            }
        }
    }

    /// <inheritdoc/>
    internal override void PerformLayout()
    {
        LayoutControls();
    }
}