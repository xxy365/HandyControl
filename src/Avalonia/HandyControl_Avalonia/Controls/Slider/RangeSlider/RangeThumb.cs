using Avalonia;
using Avalonia.Controls.Primitives;

namespace HandyControl.Controls;

public class RangeThumb : Thumb
{
    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<RangeThumb, object?>(
        nameof(Content));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}