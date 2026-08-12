using System;
using Avalonia.Controls;
using Avalonia.Media;
using HandyControl.Controls;
using HandyControl.Data;

namespace HandyControlDemo.UserControl;

public partial class ScreenshotDemo : Avalonia.Controls.UserControl, IDisposable
{
    public ScreenshotDemo()
    {
        InitializeComponent();
        Screenshot.Snapped += Screenshot_Snapped;
    }

    private void StartScreenshot_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new Screenshot().Start();
    }

    private void Screenshot_Snapped(object? sender, FunctionEventArgs<IImage> e)
    {
        var result = new Avalonia.Controls.Window
        {
            Content = new Image
            {
                Source = e.Info,
                Stretch = Stretch.None
            },
            Width = e.Info.Size.Width + 16,
            Height = e.Info.Size.Height + 16,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        if (TopLevel.GetTopLevel(this) is Avalonia.Controls.Window owner)
        {
            result.ShowDialog(owner);
        }
        else
        {
            result.Show();
        }
    }

    public void Dispose() => Screenshot.Snapped -= Screenshot_Snapped;
}