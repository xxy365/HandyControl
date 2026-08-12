using Avalonia;

namespace HandyControl.Controls;

public class HeaderedSimpleItemsControl : SimpleItemsControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<HeaderedSimpleItemsControl, object?>(nameof(Header));

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}