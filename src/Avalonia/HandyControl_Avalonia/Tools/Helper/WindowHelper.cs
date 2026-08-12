using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace HandyControl.Tools.Helper;

public static class WindowHelper
{
    public const string WindowMaximizedPadding = "0,0,0,0";

    public static Avalonia.Controls.Window? GetActiveWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
            {
                Windows: { } windows
            })
        {
            foreach (var window in windows)
            {
                if (window.IsActive)
                {
                    return window;
                }
            }

            if (windows.Count > 0)
            {
                return windows[0];
            }
        }

        return null;
    }
}