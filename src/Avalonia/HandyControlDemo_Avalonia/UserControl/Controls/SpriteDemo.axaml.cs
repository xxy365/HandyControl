using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using HandyControl.Controls;

namespace HandyControlDemo.UserControl;

public partial class SpriteDemo : Avalonia.Controls.UserControl
{
    public SpriteDemo()
    {
        InitializeComponent();
    }

    private void Show_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var content = new Border
        {
            Width = 200,
            Height = 80,
            Background = new SolidColorBrush(Color.Parse("#FFF06632")),
            CornerRadius = new CornerRadius(4),
            Child = new TextBlock
            {
                Text = "Sprite",
                FontSize = 20,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        Sprite.Show(content);
    }
}