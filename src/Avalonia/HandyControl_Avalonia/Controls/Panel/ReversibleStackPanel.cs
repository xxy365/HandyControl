using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace HandyControl.Controls;

public class ReversibleStackPanel : StackPanel
{
    public static readonly StyledProperty<bool> ReverseOrderProperty =
        AvaloniaProperty.Register<ReversibleStackPanel, bool>(nameof(ReverseOrder));

    public bool ReverseOrder
    {
        get => GetValue(ReverseOrderProperty);
        set => SetValue(ReverseOrderProperty, value);
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (!ReverseOrder)
        {
            return base.ArrangeOverride(arrangeSize);
        }

        var rcChild = new Rect(arrangeSize);
        double previousChildSize;

        if (Orientation == Orientation.Horizontal)
        {
            rcChild = rcChild.WithX(arrangeSize.Width);

            foreach (var child in Children)
            {
                if (!child.IsVisible) continue;
                previousChildSize = child.DesiredSize.Width;
                rcChild = rcChild.WithX(rcChild.X - previousChildSize);
                rcChild = rcChild.WithWidth(previousChildSize);
                rcChild = rcChild.WithHeight(Math.Max(arrangeSize.Height, child.DesiredSize.Height));

                child.Arrange(rcChild);
            }
        }
        else
        {
            rcChild = rcChild.WithY(arrangeSize.Height);

            foreach (var child in Children)
            {
                if (!child.IsVisible) continue;
                previousChildSize = child.DesiredSize.Height;
                rcChild = rcChild.WithY(rcChild.Y - previousChildSize);
                rcChild = rcChild.WithHeight(previousChildSize);
                rcChild = rcChild.WithWidth(Math.Max(arrangeSize.Width, child.DesiredSize.Width));

                child.Arrange(rcChild);
            }
        }

        return arrangeSize;
    }
}
