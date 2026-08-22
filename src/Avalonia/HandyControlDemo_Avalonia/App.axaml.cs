using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using HandyControlDemo.Tools;


namespace HandyControlDemo;

public class App : Application
{
    public override void Initialize()
    {
        DebugLog.Log("App: Initialize");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            DebugLog.LogException("AppDomain.UnhandledException (terminating=" + e.IsTerminating + ")",
                e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()));

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            DebugLog.LogException("TaskScheduler.UnobservedTaskException", e.Exception);
            e.SetObserved();
        };

        Avalonia.Threading.Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            DebugLog.LogException("Dispatcher.UIThread.UnhandledException", e.Exception);
            e.Handled = true;
        };

        DebugLog.Log("App: OnFrameworkInitializationCompleted");
        AppSettings.Load();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += (_, _) => DebugLog.Log("ShutdownRequested");
        }

        if (AppSettings.ThemeVariant is "Dark")
        {
            RequestedThemeVariant = ThemeVariant.Dark;
        }
        else if (AppSettings.ThemeVariant is "Light")
        {
            RequestedThemeVariant = ThemeVariant.Light;
        }

        Properties.Langs.LangProvider.Culture = new CultureInfo(AppSettings.Language);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop2)
        {
            desktop2.MainWindow = new MainWindow();
            desktop2.MainWindow.AttachDevTools(new Avalonia.Diagnostics.DevToolsOptions
            {
                Gesture = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.F12)
            });
            DebugLog.Log("App: DevTools attached (F12)");
        }

        base.OnFrameworkInitializationCompleted();
    }
}
