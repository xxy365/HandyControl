using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;

namespace HandyControl.Controls;

public class Empty : ContentControl
{
    public Empty()
    {
        var logo = new Path
        {
            Width = 64,
            Height = 41,
            Stretch = Stretch.Uniform,
            Fill = Brushes.Gray,
            Data = StreamGeometry.Parse("M0,0h64v23h-64z M6,4v15h52v-15z M10,8h44v2h-44z M10,12h30v2h-30z M10,16h22v2h-22z")
        };
        SetValue(LogoProperty, logo);

        var description = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
            Foreground = Brushes.Gray,
            Text = Properties.Langs.Lang.NoData
        };
        SetValue(DescriptionProperty, description);
    }

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<Empty, object?>(nameof(Description));

    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly StyledProperty<object?> LogoProperty =
        AvaloniaProperty.Register<Empty, object?>(nameof(Logo));

    public object? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }

    public static readonly AttachedProperty<bool> ShowEmptyProperty =
        AvaloniaProperty.RegisterAttached<Empty, AvaloniaObject, bool>("ShowEmpty", inherits: true);

    public static void SetShowEmpty(AvaloniaObject element, bool value)
        => element.SetValue(ShowEmptyProperty, value);

    public static bool GetShowEmpty(AvaloniaObject element)
        => element.GetValue(ShowEmptyProperty);
}