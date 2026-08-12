using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace HandyControl.Controls;

public class WatermarkTextBox : TextBox
{
    public new static readonly StyledProperty<object?> WatermarkProperty =
        AvaloniaProperty.Register<WatermarkTextBox, object?>(nameof(Watermark));

    public new object? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (IsEnabled && !string.IsNullOrEmpty(Text))
        {
            SelectAll();
        }
    }
}
