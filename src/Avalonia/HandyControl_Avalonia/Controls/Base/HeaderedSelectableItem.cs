using Avalonia;
using Avalonia.Controls.Templates;

namespace HandyControl.Controls;

public class HeaderedSelectableItem : SelectableItem
{
    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<HeaderedSelectableItem, object?>(
        nameof(Header));

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<HeaderedSelectableItem, IDataTemplate?>(nameof(HeaderTemplate));

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }
}