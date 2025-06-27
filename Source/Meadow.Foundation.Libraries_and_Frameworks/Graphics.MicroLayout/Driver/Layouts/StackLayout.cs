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
    /// <param name="crossAxisAlignment">The alignment of controls perpendicular to the stacking direction.</param>
    public StackLayout(int left, int top, int width, int height,
                       Orientation orientation = Orientation.Vertical,
                       CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Center)
        : base(left, top, width, height)
    {
        _stackOrientation = orientation;
        _crossAxisAlignment = crossAxisAlignment;

        // Subscribing to ControlAdded/Removed ensures a re-layout when children change.
        Controls.ControlAdded += OnControlsChanged;
        Controls.ControlRemoved += OnControlsChanged;
    }

    private void OnControlsChanged(object sender, IControl e)
    {
        LayoutControls();
        Invalidate();
    }

    /// <summary>
    /// Arranges child controls based on the stack orientation and cross-axis alignment.
    /// </summary>
    /// <summary>
    /// Arranges child controls based on the stack orientation and cross-axis alignment.
    /// </summary>
    public void LayoutControls()
    {
        int currentMainAxisOffset = 0; // Tracks position along the main stacking axis relative to this StackLayout's (0,0)

        lock (Controls.SyncRoot) // Ensure thread safety when iterating Controls
        {
            foreach (var control in Controls)
            {
                if (!control.IsVisible)
                {
                    continue; // Skip invisible controls in layout calculations
                }

                int finalControlX;
                int finalControlY;
                int finalControlWidth = control.Width;  // Start with control's intrinsic width
                int finalControlHeight = control.Height; // Start with control's intrinsic height

                if (StackOrientation == Orientation.Vertical)
                {
                    // Vertical stacking: Main axis is Y (Top). Cross axis is X (Left).
                    // finalControlY is relative to the StackLayout's Top
                    finalControlY = currentMainAxisOffset;

                    // Determine finalControlWidth and finalControlX based on CrossAxisAlignment (horizontal alignment)
                    // These positions are relative to the StackLayout's own (0,0)
                    switch (AxisAlignment)
                    {
                        case CrossAxisAlignment.Start:
                            finalControlX = 0; // Align to the left edge of the stack layout's content area
                            break;
                        case CrossAxisAlignment.Center:
                            finalControlX = (Width / 2) - (finalControlWidth / 2); // Center horizontally within the stack layout's content area
                            break;
                        case CrossAxisAlignment.End:
                            finalControlX = Width - finalControlWidth; // Align to the right edge of the stack layout's content area
                            break;
                        case CrossAxisAlignment.Stretch:
                            finalControlX = 0;
                            finalControlWidth = Width; // Stretch to fill the full width of the stack layout
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(CrossAxisAlignment), "Unknown CrossAxisAlignment value.");
                    }
                    currentMainAxisOffset += finalControlHeight + Spacing; // Advance offset by current control's (potentially stretched) height
                }
                else // Orientation.Horizontal
                {
                    // Horizontal stacking: Main axis is X (Left). Cross axis is Y (Top).
                    // finalControlX is relative to the StackLayout's Left
                    finalControlX = currentMainAxisOffset;

                    // Determine finalControlHeight and finalControlY based on CrossAxisAlignment (vertical alignment)
                    // These positions are relative to the StackLayout's own (0,0)
                    switch (AxisAlignment)
                    {
                        case CrossAxisAlignment.Start:
                            finalControlY = 0; // Align to the top edge of the stack layout's content area
                            break;
                        case CrossAxisAlignment.Center:
                            finalControlY = (Height / 2) - (finalControlHeight / 2); // Center vertically within the stack layout's content area
                            break;
                        case CrossAxisAlignment.End:
                            finalControlY = Height - finalControlHeight; // Align to the bottom edge of the stack layout's content area
                            break;
                        case CrossAxisAlignment.Stretch:
                            finalControlY = 0;
                            finalControlHeight = Height; // Stretch to fill the full height of the stack layout
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(CrossAxisAlignment), "Unknown CrossAxisAlignment value.");
                    }
                    currentMainAxisOffset += finalControlWidth + Spacing; // Advance offset by current control's (potentially stretched) width
                }

                // Apply the calculated dimensions and positions to the control
                // These are correctly relative to the StackLayout's (0,0)
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