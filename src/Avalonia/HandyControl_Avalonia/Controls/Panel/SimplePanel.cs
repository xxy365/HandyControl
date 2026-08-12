using System;
using Avalonia;
using Avalonia.Controls;

namespace HandyControl.Controls;

/// <summary>
///     用以代替Grid
/// </summary>
/// <remarks>
///     当不需要Grid的行、列分隔等功能时建议用此轻量级类代替
/// </remarks>
public class SimplePanel : Panel
{
    protected override Size MeasureOverride(Size constraint)
    {
        var maxSize = new Size();

        foreach (var child in Children)
        {
            if (child != null)
            {
                child.Measure(constraint);
                maxSize = maxSize.WithWidth(Math.Max(maxSize.Width, child.DesiredSize.Width));
                maxSize = maxSize.WithHeight(Math.Max(maxSize.Height, child.DesiredSize.Height));
            }
        }

        return maxSize;
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        foreach (var child in Children)
        {
            child?.Arrange(new Rect(arrangeSize));
        }

        return arrangeSize;
    }
}
