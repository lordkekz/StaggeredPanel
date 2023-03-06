using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using static System.Math;

namespace StaggeredPanel;

/// <summary>
/// Positions child elements in several stacks depending on the value of the <see cref="Orientation"/> property.
/// Each item is placed at the current shortest stack.
/// </summary>
public class StaggeredPanel : Panel {
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<StaggeredPanel, Orientation>(nameof(Orientation),
            defaultValue: Orientation.Horizontal);

    /// <summary>
    /// Defines the <see cref="StackCount"/> property.
    /// </summary>
    public static readonly StyledProperty<int?> StackCountProperty =
        AvaloniaProperty.Register<StaggeredPanel, int?>(nameof(StackCount));

    /// <summary>
    /// Defines the <see cref="StackWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double?> StackWidthProperty =
        AvaloniaProperty.Register<StaggeredPanel, double?>(nameof(StackWidth));

    /// <summary>
    /// Initializes static members of the <see cref="StaggeredPanel"/> class.
    /// </summary>
    static StaggeredPanel() {
        AffectsMeasure<StaggeredPanel>(OrientationProperty, StackCountProperty, StackWidthProperty);
    }

    /// <summary>
    /// Gets or sets the orientation in which child controls will be layed out.
    /// </summary>
    public Orientation Orientation {
        get { return GetValue(OrientationProperty); }
        set { SetValue(OrientationProperty, value); }
    }

    /// <summary>
    /// Gets or sets the width of all items in the StaggeredPanel.
    /// </summary>
    public double? StackWidth {
        get { return GetValue(StackWidthProperty); }
        set { SetValue(StackWidthProperty, value); }
    }

    /// <summary>
    /// Gets or sets the width of all items in the StaggeredPanel.
    /// </summary>
    public int? StackCount {
        get { return GetValue(StackCountProperty); }
        set { SetValue(StackCountProperty, value); }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change) {
        base.OnPropertyChanged(change);
        if (change.Property == StackWidthProperty && StackCount is not null)
            throw new InvalidOperationException("StackWidth can only be specified when StackCount is null.");
        if (change.Property == StackCountProperty && StackWidth is not null)
            throw new InvalidOperationException("StackCount can only be specified when StackWidth is null.");
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size constraint) {
        var uvConstraint = new UVSize(Orientation, constraint.Width, constraint.Height);
        var isHorizontal = Orientation == Orientation.Horizontal;

        var itemV = StackWidth ?? uvConstraint.V / StackCount ?? uvConstraint.V;

        foreach (var child in Children)
            child.Measure(new Size(
                isHorizontal ? constraint.Width : itemV,
                isHorizontal ? itemV : constraint.Height));
        return constraint;
    }


    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize) {
        var spacing = 8d;
        var uvFinalSize = new UVSize(Orientation, finalSize.Width, finalSize.Height);
        var isHorizontal = Orientation == Orientation.Horizontal;

        // itemV is either StackWidth or calculated from StackCount or just the entire size.
        var itemV = StackWidth ?? (uvFinalSize.V - (StackCount - 1) * spacing) / StackCount ?? uvFinalSize.V;
        // stackCount is calculated from itemV; it equals StackCount if itemV was calculated thanks to integer cast.
        int stackCount = (int)Max(1.1 + (uvFinalSize.V - itemV) / (itemV + spacing), 1);
        // Re-Calculate itemV to fill gaps.
        itemV = (uvFinalSize.V - (stackCount - 1) * spacing) / stackCount;


        // Init stacks
        var stacks = new List<UStack>();
        for (var i = 0; i < stackCount; i++)
            stacks.Add(new UStack(Orientation));

        // Put children into stacks
        foreach (var child in Children)
            stacks.MinBy(s => s.TotalU).PutChild(child);

        // Arrange the children of each stack
        var v = 0d;
        foreach (var stack in stacks) {
            var u = 0d;
            foreach (var child in stack.Controls) {
                var r = new Rect(
                    isHorizontal ? u : v,
                    isHorizontal ? v : u,
                    isHorizontal ? child.DesiredSize.Width : itemV,
                    isHorizontal ? itemV : child.DesiredSize.Height);
                // Debug.WriteLine($"Stack {stacks.IndexOf(stack)}, Child {stack.Controls.IndexOf(child)} " +
                //                 $"(global {Children.IndexOf(child)}) got Rect {r}");
                child.Arrange(r);
                u += spacing + (isHorizontal ? child.DesiredSize.Width : child.DesiredSize.Height);
            }

            v += itemV + spacing;
        }
        
        return finalSize;
    }

    /// <summary>
    /// Represents a Size which tracks both global width and height as well as orientation-depentent U and V.
    /// </summary>
    private struct UVSize {
        internal UVSize(Orientation orientation, double width, double height) {
            U = V = 0d;
            _orientation = orientation;
            Width = width;
            Height = height;
        }

        internal double U;
        internal double V;
        private Orientation _orientation;

        internal double Width {
            get { return _orientation == Orientation.Horizontal ? U : V; }
            set {
                if (_orientation == Orientation.Horizontal) U = value;
                else V = value;
            }
        }

        internal double Height {
            get { return _orientation == Orientation.Horizontal ? V : U; }
            set {
                if (_orientation == Orientation.Horizontal) V = value;
                else U = value;
            }
        }
    }

    /// <summary>
    /// Represents a Stack of controls along U direction.
    /// </summary>
    private class UStack {
        private readonly Orientation _orientation;

        public UStack(Orientation orientation) {
            _orientation = orientation;
        }

        public List<IControl> Controls { get; } = new();
        public double TotalU { get; private set; }

        public void PutChild(IControl child) {
            Controls.Add(child);
            TotalU += _orientation == Orientation.Horizontal ? child.DesiredSize.Width : child.DesiredSize.Height;
        }
    }
}