using System;
using Avalonia.Controls;

namespace HandyControl.Controls;

internal class SpeTextBlock : TextBlock
{
    public SpeTextBlock() => Width = 60;

    public double X { get; set; }

    private DateTime _time;

    public DateTime Time
    {
        get => _time;
        set
        {
            _time = value;
            Text = $"{value.ToString(TimeFormat)}\r\n|";
        }
    }

    public string TimeFormat { get; set; } = "HH:mm";

    public void MoveX(double offsetX) => Canvas.SetLeft(this, X + offsetX);
}