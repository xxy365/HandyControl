using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;


namespace HandyControlDemo;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        AppSettings.Load();

        if (AppSettings.ThemeVariant is "Dark")
        {
            RequestedThemeVariant = ThemeVariant.Dark;
        }
        else if (AppSettings.ThemeVariant is "Light")
        {
            RequestedThemeVariant = ThemeVariant.Light;
        }

        Properties.Langs.LangProvider.Culture = new CultureInfo(AppSettings.Language);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
