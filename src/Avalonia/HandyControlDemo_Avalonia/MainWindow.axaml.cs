using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using HandyControl.Controls;
using HandyControlDemo.Data;
using HandyControlDemo.Tools;
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

    private bool _codeEditorInitialized;
    private string? _currentDemoKey;
    private Dictionary<string, TextEditor>? _textEditors;
    private TranslateTransform? _codePanelTransform;

    public MainWindow()
    {
        DebugLog.SessionStart();
        DebugLog.Log("MainWindow: ctor begin");

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
        DebugLog.Log("MainWindow: OnLoaded, subscribing IsCodeOpened");
        DataContext = ViewModelLocator.Instance.Main;
        ControlMain.Content = new MainWindowContent();
        _codePanelTransform = CodePanel.RenderTransform as TranslateTransform;

        ViewModelLocator.Instance.Main.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.IsCodeOpened))
            {
                DebugLog.Log($"MainWindow: PropertyChanged IsCodeOpened = {ViewModelLocator.Instance.Main.IsCodeOpened}");
                ToggleCodePanel(ViewModelLocator.Instance.Main.IsCodeOpened);
            }
        };
    }

    private bool _codePanelOpen;

    private void ToggleCodePanel(bool open)
    {
        DebugLog.Log($"ToggleCodePanel: open={open}, initialized={_codeEditorInitialized}");
        _codePanelOpen = open;

        if (open)
        {
            if (!_codeEditorInitialized)
            {
                try
                {
                    InitCodeEditor();
                }
                catch (Exception ex)
                {
                    DebugLog.LogException("InitCodeEditor", ex);
                }
                _codeEditorInitialized = true;
            }

            UpdateCodeEditor();

            CodeOverlay.IsVisible = true;
            CodePanel.IsVisible = true;
            AnimateCodePanel(800, 0);
        }
        else
        {
            AnimateCodePanel(0, 800, () =>
            {
                CodeOverlay.IsVisible = false;
                CodePanel.IsVisible = false;
            });
        }
    }

    private void AnimateCodePanel(double from, double to, Action? onComplete = null)
    {
        const int totalMs = 250;
        const int fps = 60;
        var interval = TimeSpan.FromMilliseconds(1000.0 / fps);
        var steps = (int)(totalMs / interval.TotalMilliseconds);
        var step = 0;

        void Tick()
        {
            step++;
            var t = Math.Min(1.0, (double)step / steps);
            var eased = t * t * (3 - 2 * t);
            _codePanelTransform.X = from + (to - from) * eased;

            if (step >= steps)
            {
                _codePanelTransform.X = to;
                onComplete?.Invoke();
            }
            else
            {
                Dispatcher.UIThread.Post(() => Tick(), DispatcherPriority.Render);
            }
        }

        Dispatcher.UIThread.Post(Tick, DispatcherPriority.Render);
    }

    private void CodeOverlay_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ViewModelLocator.Instance.Main.IsCodeOpened = false;
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

    private void InitCodeEditor()
    {
        var isDark = Equals(Application.Current?.ActualThemeVariant, ThemeVariant.Dark);

        var regionBrush = TryFindBrush("RegionBrush");
        IBrush editorForeground = isDark ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
        IBrush lineNumbersForeground = isDark ? new SolidColorBrush(Color.Parse("#FF929292")) : new SolidColorBrush(Colors.Black);

        TextEditor CreateEditor()
        {
            var editor = new TextEditor
            {
                IsReadOnly = true,
                ShowLineNumbers = true,
                Margin = new Thickness(4),
                FontFamily = new FontFamily("Consolas"),
                Background = regionBrush,
                Foreground = editorForeground,
                LineNumbersForeground = lineNumbersForeground
            };
            editor.TextArea.SelectionBrush = new SolidColorBrush(Color.Parse("#BF0078D7"));
            editor.TextArea.SelectionForeground = Brushes.White;
            editor.TextArea.SelectionCornerRadius = 0;
            return editor;
        }

        _textEditors = new Dictionary<string, TextEditor>
        {
            ["XAML"] = CreateEditor(),
            ["C#"] = CreateEditor(),
            ["VM"] = CreateEditor()
        };

        _textEditors["XAML"].SyntaxHighlighting = LoadHighlighting("XML", isDark);
        _textEditors["C#"].SyntaxHighlighting = LoadHighlighting("C#", isDark);
        _textEditors["VM"].SyntaxHighlighting = LoadHighlighting("C#", isDark);

        BorderCode.Child = new TabControl
        {
            Items =
            {
                new TabItem { Header = "XAML", Content = _textEditors["XAML"] },
                new TabItem { Header = "C#", Content = _textEditors["C#"] },
                new TabItem { Header = "VM", Content = _textEditors["VM"] }
            }
        };
    }

    private IBrush? TryFindBrush(string key)
    {
        if (Application.Current != null &&
            Application.Current.TryGetResource(key, Application.Current.ActualThemeVariant, out var value) &&
            value is IBrush brush)
        {
            return brush;
        }
        return null;
    }

    private void UpdateCodeEditor()
    {
        var vm = ViewModelLocator.Instance.Main;
        if (_textEditors == null)
        {
            DebugLog.Log("UpdateCodeEditor: SKIP _textEditors == null");
            return;
        }
        if (vm.DemoInfoCurrent == null || vm.DemoItemCurrent == null)
        {
            DebugLog.Log($"UpdateCodeEditor: SKIP DemoInfoCurrent={(vm.DemoInfoCurrent?.Key ?? "null")}, DemoItemCurrent={(vm.DemoItemCurrent?.TargetCtlName ?? "null")}");
            return;
        }

        var typeKey = vm.DemoInfoCurrent.Key;
        var demoKey = vm.DemoItemCurrent.TargetCtlName;
        if (Equals(_currentDemoKey, demoKey))
        {
            DebugLog.Log($"UpdateCodeEditor: SKIP same demoKey={demoKey}");
            return;
        }
        _currentDemoKey = demoKey;

        if (vm.SubContent is Avalonia.Controls.Control demo)
        {
            var demoTypeName = demo.GetType().Name;
            var xamlPath = $"UserControl/{typeKey}/{demoTypeName}.axaml";
            var dc = demo.DataContext;
            var dcTypeName = dc?.GetType().Name;
            var vmPath = dcTypeName != null && !Equals(dcTypeName, demoTypeName)
                ? $"ViewModel/{dcTypeName}" : xamlPath;

            var xamlCode = DemoHelper.GetCode(xamlPath);
            var csCode = DemoHelper.GetCode($"{xamlPath}.cs");
            var vmCode = DemoHelper.GetCode($"{vmPath}.cs");

            _textEditors["XAML"].Text = xamlCode;
            _textEditors["C#"].Text = csCode;
            _textEditors["VM"].Text = vmCode;

            DebugLog.Log($"UpdateCodeEditor: typeKey={typeKey}, demoType={demoTypeName}, dcType={dcTypeName ?? "null"}, xamlPath=\"{xamlPath}\", vmPath=\"{vmPath}\" => xaml={xamlCode.Length} chars, cs={csCode.Length} chars, vm={vmCode.Length} chars");
        }
        else
        {
            DebugLog.Log($"UpdateCodeEditor: SubContent is {(vm.SubContent?.GetType().FullName ?? "null")} (not a Control)");
        }
    }

    private static IHighlightingDefinition? LoadHighlighting(string name, bool isDark)
    {
        if (!isDark) return HighlightingManager.Instance.GetDefinition(name);

        var xshdName = name == "XML" ? "XML-Dark" : "CSharp-Dark";
        try
        {
            var uri = new Uri($"avares://HandyControlDemo/Resources/xshd/{xshdName}.xshd");
            using var stream = AssetLoader.Open(uri);
            using var reader = new XmlTextReader(stream);
            return HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
        catch
        {
            return HighlightingManager.Instance.GetDefinition(name);
        }
    }
}