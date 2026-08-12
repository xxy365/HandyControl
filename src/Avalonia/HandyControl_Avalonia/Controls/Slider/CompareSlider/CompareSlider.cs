using Avalonia;
using Avalonia.Controls;

namespace HandyControl.Controls;

public class CompareSlider : Slider
{
    public static readonly StyledProperty<object?> TargetContentProperty = AvaloniaProperty.Register<CompareSlider, object?>(
        nameof(TargetContent));

    public static readonly StyledProperty<object?> SourceContentProperty = AvaloniaProperty.Register<CompareSlider, object?>(
        nameof(SourceContent));

    public object? TargetContent
    {
        get => GetValue(TargetContentProperty);
        set => SetValue(TargetContentProperty, value);
    }

    public object? SourceContent
    {
        get => GetValue(SourceContentProperty);
        set => SetValue(SourceContentProperty, value);
    }
}