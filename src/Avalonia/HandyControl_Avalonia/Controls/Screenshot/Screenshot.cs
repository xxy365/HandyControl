using System;
using Avalonia.Media;
using HandyControl.Data;

namespace HandyControl.Controls;

public class Screenshot
{
    public static event EventHandler<FunctionEventArgs<IImage>>? Snapped;

    public void Start() => new ScreenshotWindow(this).Show();

    internal void OnSnapped(IImage source) => Snapped?.Invoke(this, new FunctionEventArgs<IImage>(source));
}