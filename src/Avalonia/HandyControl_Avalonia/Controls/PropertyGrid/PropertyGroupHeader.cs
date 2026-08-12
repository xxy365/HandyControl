using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace HandyControl.Controls;

public class PropertyGroupHeader : ContentControl
{
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<PropertyGroupHeader, string?>(nameof(Header));

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}
