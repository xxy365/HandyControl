using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using HandyControl.Data;

namespace HandyControl.Controls;

public sealed class GrowlWindow : Avalonia.Controls.Window
{
    internal Panel GrowlPanel { get; set; }

    internal GrowlWindow()
    {
        WindowDecorations = WindowDecorations.None;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        Background = Brushes.Transparent;
        Width = 340;
        MaxWidth = 340;
        ShowActivated = false;
        ShowInTaskbar = false;
        Topmost = true;

        GrowlPanel = new ReversibleStackPanel();
        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            Content = GrowlPanel
        };
    }

    internal void UpdatePosition(TransitionMode transitionMode)
    {
        var screens = Screens.ScreenFromWindow(this);
        var workingArea = screens?.WorkingArea ?? new PixelRect(0, 0, 1920, 1080);
        Height = workingArea.Height;
        Position = new PixelPoint(0, 0);

        var panelHorizontalAlignment = Growl.GetPanelHorizontalAlignment(transitionMode);
        var left = panelHorizontalAlignment switch
        {
            HorizontalAlignment.Right => workingArea.Right - Width,
            HorizontalAlignment.Left => workingArea.X,
            HorizontalAlignment.Center => workingArea.X + (workingArea.Width - Width) * 0.5,
            _ => workingArea.Right - Width
        };

        Position = new PixelPoint((int)left, 0);

        Growl.SetTransitionMode(this, transitionMode);
        GrowlPanel.SetValue(ReversibleStackPanel.ReverseOrderProperty,
            transitionMode is TransitionMode.Bottom2Top or TransitionMode.Bottom2TopWithFade);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        UpdatePosition(Growl.GetTransitionMode(this));
    }
}
