using System;
using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A base class for display layouts
/// </summary>
public abstract class LayoutBase : ThemedControl, ILayout
{
    /// <summary>
    /// Gets the collection of controls on the display screen.
    /// </summary>
    public virtual ControlsCollection Controls { get; private set; }

    /// <inheritdoc/>
    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            if (base.IsVisible == value) return;

            base.IsVisible = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the background color of the Layout.
    /// </summary>
    public Color? BackgroundColor
    {
        get => _backgroundColor;
        set => SetInvalidatingProperty(ref _backgroundColor, value);
    }

    /// <inheritdoc/>
    /// A layout is only invalid if its OWN properties changed, not if children are invalid.
    /// The dirty region calculation will find invalid children directly.
    public override bool IsInvalid => base.IsInvalid;

    private Color? _backgroundColor;

    /// <summary>
    /// Creates a LayoutBase
    /// </summary>
    protected LayoutBase() : this(0, 0, 0, 0) { }

    /// <summary>
    /// Creates a LayoutBase
    /// </summary>
    /// <param name="left">The layout's left position</param>
    /// <param name="top">The layout's top position</param>
    /// <param name="width">The layout's width</param>
    /// <param name="height">The layout's height</param>
    protected LayoutBase(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        Controls = new ControlsCollection(this);
        Controls.ControlAdded += OnChildrenCollectionChanged;
        Controls.ControlRemoved += OnChildrenCollectionChanged;
    }

    /// <summary>
    /// Performs layout of the controls within the layout
    /// </summary>
    internal abstract void PerformLayout();

    /// <inheritdoc/>
    public override void ApplyTheme(DisplayTheme theme)
    {
        lock (Controls.SyncRoot)
        {
            foreach (var control in Controls.OfType<IThemedControl>())
            {
                control.ApplyTheme(theme);
            }

            if (theme.BackgroundColor != null)
            {
                BackgroundColor = theme.BackgroundColor;
            }
        }
        Invalidate();
    }

    /// <inheritdoc/>
    public override void Invalidate()
    {
        // Debug: Log when AbsoluteLayout is invalidated
        if (this.GetType().Name == "AbsoluteLayout")
        {
            Meadow.Resolver.Log.Info($"AbsoluteLayout.Invalidate() called from:\n{new System.Diagnostics.StackTrace()}", "MicroLayout");
        }

        // Only invalidate the layout itself, not all children.
        // Children will invalidate themselves when their own properties change.
        // If we need to invalidate children (e.g., due to layout bounds changing),
        // OnParentBoundsChanged will handle that automatically.
        base.Invalidate();
    }

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (!IsVisible) { return; }

        if (BackgroundColor != null && BackgroundColor != Color.Transparent)
        {
            graphics.DrawRectangle(ScreenLeft, ScreenTop, Width, Height, BackgroundColor.Value, true);
        }

        foreach (var control in Controls)
        {
            control.Refresh(graphics);
        }
    }

    /// <inheritdoc />
    protected override void OnParentBoundsChanged(object? sender, EventArgs e)
    {
        base.OnParentBoundsChanged(sender, e);
        PerformLayout();
    }

    /// <inheritdoc />
    protected virtual void OnChildrenCollectionChanged(object? sender, IControl control)
    {
        PerformLayout();
        Invalidate();
    }
}