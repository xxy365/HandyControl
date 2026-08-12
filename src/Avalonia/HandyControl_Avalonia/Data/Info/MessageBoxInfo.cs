using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace HandyControl.Data;

public class MessageBoxInfo
{
    public string? Message { get; set; }

    public string? Caption { get; set; }

    public MessageBoxButton Button { get; set; } = MessageBoxButton.OK;

    public Geometry? Icon { get; set; }

    public string? IconKey { get; set; }

    public IBrush? IconBrush { get; set; }

    public string? IconBrushKey { get; set; }

    public MessageBoxResult DefaultResult { get; set; } = MessageBoxResult.None;

    public ControlTheme? Style { get; set; }

    public string? StyleKey { get; set; }

    public Window? Owner { get; set; }
}