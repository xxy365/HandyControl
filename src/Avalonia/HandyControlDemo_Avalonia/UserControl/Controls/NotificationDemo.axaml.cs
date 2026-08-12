using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using HandyControl.Controls;
using HandyControl.Data;

namespace HandyControlDemo.UserControl;

public partial class NotificationDemo : Avalonia.Controls.UserControl
{
    private Notification? _lastNotification;

    public NotificationDemo()
    {
        InitializeComponent();
        AnimationCombo.ItemsSource = Enum.GetValues<ShowAnimation>();
        AnimationCombo.SelectedIndex = 0;
    }

    private void Send_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var content = new Border
        {
            Width = 320,
            Height = 220,
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.Parse("#FFD0D0D0")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Child = new StackPanel
            {
                Margin = new Thickness(16),
                Orientation = Orientation.Vertical,
                Children =
                {
                    new TextBlock
                    {
                        Text = "HandyControl",
                        FontSize = 24,
                        Foreground = new SolidColorBrush(Color.Parse("#FF3E7FF2")),
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new TextBlock
                    {
                        Text = "This is a notification card. Hover to pause the close timer.",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Colors.Gray),
                        Margin = new Thickness(0, 12, 0, 0)
                    }
                }
            }
        };

        var closeButton = new Button
        {
            Content = "Close",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 16, 0, 0)
        };
        closeButton.Click += Close_OnClick;
        (content.Child as StackPanel)?.Children.Add(closeButton);

        _lastNotification = Notification.Show(content, (ShowAnimation)AnimationCombo.SelectedItem!, StaysOpenToggle.IsChecked == true);
    }

    private void Close_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _lastNotification?.Close();
    }
}