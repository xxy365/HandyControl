using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using HandyControl.Controls;
using HandyControlDemo.Data;
using HandyControlDemo.UserControl;
using HandyControlDemo.ViewModel;

namespace HandyControlDemo;

public partial class MainWindow : HandyControl.Controls.Window
{
    private static readonly (string tag, string label)[] Langs =
    {
        ("zh-cn", "中文"),
        ("en", "English"),
        ("fa", "فارسی"),
        ("fr", "Français"),
        ("ca-ES", "Català"),
        ("ja", "日本語"),
        ("ko-KR", "한국어"),
        ("ru", "Русский"),
        ("tr", "Türkçe"),
        ("pt-BR", "Português"),
        ("pl", "Polski"),
        ("es", "Español"),
        ("cs", "Čeština"),
    };

    public MainWindow()
    {
        InitializeComponent();
        Dialog.SetToken(this, MessageToken.MainWindow);

        VersionText.Text = $"v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0"}";

        MenuGitHub.Click += (_, _) => OpenUrl("https://github.com/NaBian/HandyControl");
        MenuNuGet.Click += (_, _) => OpenUrl("https://www.nuget.org/packages/HandyControl");
        MenuVsix.Click += (_, _) => OpenUrl("https://marketplace.visualstudio.com/items?itemName=HandyOrg.HandyControl");
        MenuEmail.Click += (_, _) => OpenUrl("mailto:836904362@qq.com");
        MenuChatroom.Click += (_, _) => OpenUrl("https://join.slack.com/t/handycontrol/shared_invite/zt-sw29prqd-okFmRlmETdtWhnF7C3foxA");
        MenuBlog.Click += (_, _) => OpenUrl("https://www.cnblogs.com/nabian");
        MenuDemoGitHub.Click += (_, _) => OpenUrl("https://github.com/AFei19911012/HandyControl");
        MenuWiki.Click += (_, _) => OpenUrl("https://github.com/ghost1372/HandyControl/wiki/Documentation");
        MenuDocEn.Click += (_, _) => OpenUrl("https://ghost1372.github.io");
        MenuDocCn.Click += (_, _) => OpenUrl("https://handyorg.github.io");

        BuildLangPanel();
        BuildSkinPanel();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        DataContext = ViewModelLocator.Instance.Main;
        ControlMain.Content = new MainWindowContent();
    }

    private void OpenUrl(string url)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }

    private void BuildLangPanel()
    {
        foreach (var (tag, label) in Langs)
        {
            var btn = new Button
            {
                Content = label,
                Tag = tag,
                Width = 80,
                Height = 24,
                FontSize = 12,
                Padding = new Thickness(4),
            };
            btn.Click += (_, _) =>
            {
                var lang = (string)((Button)btn).Tag!;
                ButtonConfig.Flyout?.Hide();
                Properties.Langs.LangProvider.Culture = new CultureInfo(lang);
                AppSettings.Language = lang;
                AppSettings.Save();
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                    new CommunityToolkit.Mvvm.Messaging.Messages.ValueChangedMessage<bool>(true),
                    MessageToken.LangUpdated);
            };
            LangPanel.Children.Add(btn);
        }
    }

    private void BuildSkinPanel()
    {
        var skins = new (string label, Color color, ThemeVariant? variant)[]
        {
            ("Default", Colors.White, null),
            ("Dark", Colors.Black, ThemeVariant.Dark),
            ("Light", Colors.White, ThemeVariant.Light),
        };

        foreach (var (label, color, variant) in skins)
        {
            var btn = new Button
            {
                Tag = variant,
                Width = 32,
                Height = 21,
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.Parse("#999999")),
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(2),
                Margin = new Thickness(0, label == "Default" ? 0 : 10, 0, 0),
            };
            if (variant == null)
            {
                var border = new Border
                {
                    Width = 32,
                    Height = 21,
                    CornerRadius = new CornerRadius(2),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.Parse("#999999")),
                };
                var grid = new Grid();
                grid.Children.Add(new Border { Background = Brushes.White });
                grid.Children.Add(new Border { Background = Brushes.Black, Width = 16, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right });
                border.Child = grid;
                btn.Content = border;
            }
            else
            {
                btn.Content = new Border
                {
                    Width = 32,
                    Height = 21,
                    CornerRadius = new CornerRadius(2),
                    Background = new SolidColorBrush(color),
                };
            }
            btn.Click += (_, _) =>
            {
                var v = (ThemeVariant?)((Button)btn).Tag;
                ButtonConfig.Flyout?.Hide();
                Application.Current!.RequestedThemeVariant = v;
                AppSettings.ThemeVariant = v == ThemeVariant.Dark ? "Dark" : v == ThemeVariant.Light ? "Light" : "Default";
                AppSettings.Save();
            };
            SkinPanel.Children.Add(btn);
        }
    }

    private void ButtonConfig_OnClick(object? sender, RoutedEventArgs e)
    {
        ButtonConfig.Flyout?.ShowAt((Button)sender!);
    }
}