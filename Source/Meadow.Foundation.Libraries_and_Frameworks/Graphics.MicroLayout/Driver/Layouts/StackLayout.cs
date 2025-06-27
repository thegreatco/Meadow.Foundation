using System;

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

    /// <summary>
    /// Defines how child controls are aligned on the axis perpendicular to the stacking direction.
    /// </summary>
    public enum CrossAxisAlignment
    {
        /// <summary>
        /// Aligns controls to the start of the cross-axis (Left for Vertical stack, Top for Horizontal stack).
        /// </summary>
        Start,
        /// <summary>
        /// Centers controls on the cross-axis.
        /// </summary>
        Center,
        /// <summary>
        /// Aligns controls to the end of the cross-axis (Right for Vertical stack, Bottom for Horizontal stack).
        /// </summary>
        End,
        /// <summary>
        /// Stretches controls to fill the available space on the cross-axis.
        /// </summary>
        Stretch
    }

    private Orientation _stackOrientation;
    private CrossAxisAlignment _crossAxisAlignment = CrossAxisAlignment.Center;
    private int _spacing = 2;
    private int _padding = 2;

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
    /// Gets or sets the alignment of controls perpendicular to the stacking direction.
    /// </summary>
    public CrossAxisAlignment AxisAlignment
    {
        get => _crossAxisAlignment;
        set
        {
            if (_crossAxisAlignment != value)
            {
                _crossAxisAlignment = value;
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
    /// Gets or sets the padding (or indentation) of all controls from all 4 edges.
    /// </summary>
    public int Padding
    {
        get => _padding;
        set
        {
            if (_padding == value) { return; }
            _padding = value;
            LayoutControls();
            Invalidate();
        }
    }

    /// <summary>
    /// Creates a new StackLayout.
    /// </summary>
    public StackLayout() : this(0, 0, 0, 0)
    { }

    /// <summary>
    /// Creates a new StackLayout.
    /// </summary>
    /// <param name="left">The left position of the layout.</param>
    /// <param name="top">The top position of the layout.</param>
    /// <param name="width">The width of the layout.</param>
    /// <param name="height">The height of the layout.</param>
    /// <param name="orientation">The stacking orientation.</param>
    /// <param name="crossAxisAlignment">The alignment of controls perpendicular to the stacking direction.</param>
    public StackLayout(int left, int top, int width, int height,
                       Orientation orientation = Orientation.Vertical,
                       CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Center)
        : base(left, top, width, height)
    {
        _stackOrientation = orientation;
        _crossAxisAlignment = crossAxisAlignment;

        Controls.ControlAdded += OnControlsChanged;
        Controls.ControlRemoved += OnControlsChanged;
    }

    private void OnControlsChanged(object sender, IControl e)
    {
        LayoutControls();
        Invalidate();
    }

    /// <summary>
    /// Arranges child controls based on the stack orientation and cross-axis alignment,
    /// respecting the layout's padding and spacing.
    /// </summary>
    public void LayoutControls()
    {
        int currentMainAxisOffset = Padding;

        int paddedWidth = Width - (2 * Padding);
        int paddedHeight = Height - (2 * Padding);

        if (paddedWidth < 0) paddedWidth = 0;
        if (paddedHeight < 0) paddedHeight = 0;

        lock (Controls.SyncRoot)
        {
            foreach (var control in Controls)
            {
                if (!control.IsVisible)
                {
                    continue;
                }

                int finalControlX;
                int finalControlY;
                int finalControlWidth = control.Width;
                int finalControlHeight = control.Height;

                if (StackOrientation == Orientation.Vertical)
                {
                    finalControlY = currentMainAxisOffset;

                    switch (AxisAlignment)
                    {
                        case CrossAxisAlignment.Start:
                            finalControlX = Padding;
                            break;
                        case CrossAxisAlignment.Center:
                            finalControlX = Padding + (paddedWidth / 2) - (finalControlWidth / 2);
                            break;
                        case CrossAxisAlignment.End:
                            finalControlX = Padding + paddedWidth - finalControlWidth;
                            break;
                        case CrossAxisAlignment.Stretch:
                            finalControlX = Padding;
                            finalControlWidth = paddedWidth;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(CrossAxisAlignment), "Unknown CrossAxisAlignment value.");
                    }
                    currentMainAxisOffset += finalControlHeight + Spacing;
                }
                else // Orientation.Horizontal
                {
                    finalControlX = currentMainAxisOffset;

                    switch (AxisAlignment)
                    {
                        case CrossAxisAlignment.Start:
                            finalControlY = Padding;
                            break;
                        case CrossAxisAlignment.Center:
                            finalControlY = Padding + (paddedHeight / 2) - (finalControlHeight / 2);
                            break;
                        case CrossAxisAlignment.End:
                            finalControlY = Padding + paddedHeight - finalControlHeight;
                            break;
                        case CrossAxisAlignment.Stretch:
                            finalControlY = Padding;
                            finalControlHeight = paddedHeight;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(CrossAxisAlignment), "Unknown CrossAxisAlignment value.");
                    }
                    currentMainAxisOffset += finalControlWidth + Spacing;
                }

                control.Left = finalControlX;
                control.Top = finalControlY;
                control.Width = finalControlWidth;
                control.Height = finalControlHeight;
            }
        }
    }

    /// <inheritdoc/>
    internal override void PerformLayout()
    {
        LayoutControls();
    }
}