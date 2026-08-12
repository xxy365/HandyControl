using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace HandyControl.Controls;

public class ClipGrid : Grid
{
    public static readonly StyledProperty<bool> IsClipEnabledProperty =
        AvaloniaProperty.Register<ClipGrid, bool>(nameof(IsClipEnabled), true);

    public bool IsClipEnabled
    {
        get => GetValue(IsClipEnabledProperty);
        set => SetValue(IsClipEnabledProperty, value);
    }

    static ClipGrid()
    {
        IsClipEnabledProperty.Changed.AddClassHandler<ClipGrid>((o, e) => o.OnIsClipEnabledChanged());
    }

    private void OnIsClipEnabledChanged()
    {
        ClipToBounds = IsClipEnabled;
    }
}
