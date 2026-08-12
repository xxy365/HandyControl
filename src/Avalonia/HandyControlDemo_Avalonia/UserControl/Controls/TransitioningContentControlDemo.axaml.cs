using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace HandyControlDemo.UserControl;

public partial class TransitioningContentControlDemo : Avalonia.Controls.UserControl
{
    private int _state;

    public TransitioningContentControlDemo()
    {
        InitializeComponent();
        Tcc1.Content = Build("First");
    }

    private void ButtonVisibilitySwitchOnClick(object? sender, RoutedEventArgs e)
    {
        _state++;
        Tcc1.Content = Build(_state % 2 == 0 ? "Second" : "First");
    }

    private static Border Build(string text) => new()
    {
        Width = 160,
        Height = 80,
        Background = new SolidColorBrush(Color.Parse("#3E82F7")),
        Child = new TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(Colors.White),
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        }
    };
}