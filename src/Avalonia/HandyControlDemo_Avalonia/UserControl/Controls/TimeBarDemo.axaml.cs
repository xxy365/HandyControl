using System;
using Avalonia.Controls;
using HandyControl.Data;

namespace HandyControlDemo.UserControl;

public partial class TimeBarDemo : Avalonia.Controls.UserControl
{
    public TimeBarDemo()
    {
        InitializeComponent();
        Loaded += TimeBarDemo_Loaded;
        TimeBarControl.TimeChanged += TimeBarControl_TimeChanged;
    }

    private void TimeBarDemo_Loaded(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

        TimeBarControl.Hotspots.Add(new DateTimeRange(start.AddHours(9), start.AddHours(10)));
        TimeBarControl.Hotspots.Add(new DateTimeRange(start.AddHours(13), start.AddHours(14)));
        TimeBarControl.Hotspots.Add(new DateTimeRange(start.AddHours(20), start.AddHours(21)));
    }

    private void TimeBarControl_TimeChanged(object? sender, FunctionEventArgs<DateTime> e)
    {
        SelectedTimeText.Text = e.Info.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private void Button_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Content: string text } && DateTime.TryParse(text, out var time))
        {
            var now = TimeBarControl.SelectedTime;
            TimeBarControl.SelectedTime = new DateTime(now.Year, now.Month, now.Day, time.Hour, time.Minute, 0);
            SelectedTimeText.Text = TimeBarControl.SelectedTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}