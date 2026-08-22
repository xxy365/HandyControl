using Avalonia;
using Avalonia.Controls;
using HandyControl.Data;

namespace HandyControl.Controls;

/// <summary>
///     规则ItemsControl
/// </summary>
/// <remarks>
///     该类的每一项都具有相同的大小和外边距
/// </remarks>
public class RegularItemsControl : SimpleItemsControl
{
    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<RegularItemsControl, double>(nameof(ItemWidth), 200);

    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<RegularItemsControl, double>(nameof(ItemHeight), 200);

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public static readonly StyledProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.Register<RegularItemsControl, Thickness>(nameof(ItemMargin));

    public Thickness ItemMargin
    {
        get => GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }
}