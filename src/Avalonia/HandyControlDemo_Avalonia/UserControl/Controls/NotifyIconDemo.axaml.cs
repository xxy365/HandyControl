using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace HandyControlDemo.UserControl;

public partial class NotifyIconDemo : Avalonia.Controls.UserControl
{
    public NotifyIconDemo()
    {
        InitializeComponent();
    }

    private void MenuHome_OnClick(object? sender, EventArgs e)
    {
    }

    private void MenuExit_OnClick(object? sender, EventArgs e) =>
        Application.Current?.TryGetFeature<IClassicDesktopStyleApplicationLifetime>()?.Shutdown();
}