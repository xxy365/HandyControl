using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HandyControl.Data;
using HandyControl.Properties.Langs;
using HandyControl.Tools;

namespace HandyControl.Controls;

/// <summary>
///     消息提醒
/// </summary>
[TemplatePart(ElementPanelMore, typeof(Panel))]
[TemplatePart(ElementGridMain, typeof(Grid))]
[TemplatePart(ElementButtonClose, typeof(Button))]
[TemplatePart(ElementButtonCancel, typeof(Button))]
[TemplatePart(ElementButtonConfirm, typeof(Button))]
public class Growl : ContentControl
{
    private const string ElementPanelMore = "PART_PanelMore";
    private const string ElementGridMain = "PART_GridMain";
    private const string ElementButtonClose = "PART_ButtonClose";
    private const string ElementButtonCancel = "PART_ButtonCancel";
    private const string ElementButtonConfirm = "PART_ButtonConfirm";
    private const int MinWaitTime = 2;
    private const double TransitionMilliseconds = 300;

    private static GrowlWindow? GrowlWindow;

    private static readonly ControlTokenManager<Panel> TokenManager =
        new(registerCallback: OnTokenRegistered, unregisterCallback: OnTokenUnregistered);

    public static readonly AttachedProperty<bool> GrowlParentProperty =
        AvaloniaProperty.RegisterAttached<Growl, AvaloniaObject, bool>("GrowlParent", false);

    static Growl()
    {
        GrowlParentProperty.Changed.AddClassHandler<AvaloniaObject>(OnGrowlParentChanged);
        TokenProperty.Changed.AddClassHandler<AvaloniaObject>(TokenManager.OnTokenChanged);
    }

    private static void OnGrowlParentChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if ((bool?)e.NewValue == true && d is Panel panel)
        {
            SetGrowlPanel(panel);
        }
    }

    public static readonly AttachedProperty<GrowlShowMode> ShowModeProperty =
        AvaloniaProperty.RegisterAttached<Growl, AvaloniaObject, GrowlShowMode>("ShowMode", inherits: true);

    public static readonly AttachedProperty<TransitionMode> TransitionModeProperty =
        AvaloniaProperty.RegisterAttached<Growl, AvaloniaObject, TransitionMode>("TransitionMode", inherits: true);

    public static readonly AttachedProperty<string?> TokenProperty =
        AvaloniaProperty.RegisterAttached<Growl, AvaloniaObject, string?>("Token");

    public static readonly StyledProperty<bool> ShowDateTimeProperty =
        AvaloniaProperty.Register<Growl, bool>(nameof(ShowDateTime), true);

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<Growl, string?>(nameof(Message));

    public static readonly StyledProperty<DateTime> TimeProperty =
        AvaloniaProperty.Register<Growl, DateTime>(nameof(Time));

    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<Growl, Geometry?>(nameof(Icon));

    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<Growl, IBrush?>(nameof(IconBrush));

    public static readonly StyledProperty<InfoType> TypeProperty =
        AvaloniaProperty.Register<Growl, InfoType>(nameof(Type));

    internal static readonly StyledProperty<string?> CancelStrProperty =
        AvaloniaProperty.Register<Growl, string?>(nameof(CancelStr));

    internal static readonly StyledProperty<string?> ConfirmStrProperty =
        AvaloniaProperty.Register<Growl, string?>(nameof(ConfirmStr));

    private static readonly AttachedProperty<bool> IsCreatedAutomaticallyProperty =
        AvaloniaProperty.RegisterAttached<Growl, AvaloniaObject, bool>("IsCreatedAutomatically");

    private Panel? _panelMore;
    private Grid? _gridMain;
    private Button? _buttonClose;
    private Button? _buttonCancel;
    private Button? _buttonConfirm;
    private bool _showCloseButton;
    private bool _staysOpen;
    private int _waitTime = 6;
    private int _tickCount;
    private DispatcherTimer? _timerClose;
    private CancellationTokenSource? _transitionCts;

    /// <summary>
    ///     消息容器
    /// </summary>
    public static Panel? GrowlPanel { get; set; }

    public InfoType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public bool ShowDateTime
    {
        get => GetValue(ShowDateTimeProperty);
        set => SetValue(ShowDateTimeProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public DateTime Time
    {
        get => GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    internal string? CancelStr
    {
        get => GetValue(CancelStrProperty);
        set => SetValue(CancelStrProperty, value);
    }

    internal string? ConfirmStr
    {
        get => GetValue(ConfirmStrProperty);
        set => SetValue(ConfirmStrProperty, value);
    }

    private Func<bool, bool>? ActionBeforeClose { get; set; }

    private static void OnTokenRegistered(string token, Panel panel)
    {
        InitGrowlPanel(panel);
    }

    private static void OnTokenUnregistered(string token, Panel panel)
    {
        panel.ContextMenu = null;
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (_buttonClose != null)
        {
            _buttonClose.IsVisible = _showCloseButton;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (_buttonClose != null)
        {
            _buttonClose.IsVisible = false;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _panelMore = e.NameScope.Find<Panel>(ElementPanelMore);
        _gridMain = e.NameScope.Find<Grid>(ElementGridMain);
        _buttonClose = e.NameScope.Find<Button>(ElementButtonClose);
        _buttonCancel = e.NameScope.Find<Button>(ElementButtonCancel);
        _buttonConfirm = e.NameScope.Find<Button>(ElementButtonConfirm);

        if (_buttonCancel != null)
        {
            _buttonCancel.Click += ButtonCancel_OnClick;
        }

        if (_buttonConfirm != null)
        {
            _buttonConfirm.Click += ButtonOk_OnClick;
        }

        CheckNull();

        IsVisible = false;
        Dispatcher.UIThread.Post(() =>
        {
            Update();
            IsVisible = true;
        });
    }

    private void CheckNull()
    {
        if (_panelMore == null || _gridMain == null || _buttonClose == null)
        {
            throw new Exception();
        }
    }

    public static void SetToken(AvaloniaObject element, string? value) => element.SetValue(TokenProperty, value);

    public static string? GetToken(AvaloniaObject element) => element.GetValue(TokenProperty);

    public static void SetShowMode(AvaloniaObject element, GrowlShowMode value) =>
        element.SetValue(ShowModeProperty, value);

    public static GrowlShowMode GetShowMode(AvaloniaObject element) =>
        element.GetValue(ShowModeProperty);

    public static void SetTransitionMode(AvaloniaObject element, TransitionMode value)
        => element.SetValue(TransitionModeProperty, value);

    public static TransitionMode GetTransitionMode(AvaloniaObject element)
        => element.GetValue(TransitionModeProperty);

    public static void SetGrowlParent(AvaloniaObject element, bool value) =>
        element.SetValue(GrowlParentProperty, value);

    public static bool GetGrowlParent(AvaloniaObject element) => element.GetValue(GrowlParentProperty);

    private static void SetIsCreatedAutomatically(AvaloniaObject element, bool value) =>
        element.SetValue(IsCreatedAutomaticallyProperty, value);

    private static bool GetIsCreatedAutomatically(AvaloniaObject element) =>
        element.GetValue(IsCreatedAutomaticallyProperty);

    /// <summary>
    ///     开始计时器
    /// </summary>
    private void StartTimer()
    {
        _timerClose = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timerClose.Tick += (_, _) =>
        {
            if (IsPointerOver)
            {
                _tickCount = 0;
                return;
            }

            _tickCount++;
            if (_tickCount >= _waitTime)
            {
                Close(true);
            }
        };
        _timerClose.Start();
    }

    /// <summary>
    ///     消息容器
    /// </summary>
    private static void SetGrowlPanel(Panel panel)
    {
        GrowlPanel = panel;
        InitGrowlPanel(panel);
    }

    private static void InitGrowlPanel(Panel panel)
    {
        if (panel == null) return;

        var menuItem = new MenuItem
        {
            Header = Lang.Clear
        };

        menuItem.Click += (_, _) => Clear(panel);
        panel.ContextMenu = new ContextMenu
        {
            Items =
            {
                menuItem
            }
        };
    }

    private void Update()
    {
        if (Type == InfoType.Ask)
        {
            _panelMore!.IsEnabled = true;
            _panelMore.IsVisible = true;
        }

        StartTransition(false);

        if (!_staysOpen)
        {
            StartTimer();
        }
    }

    private static void ShowInternal(Panel panel, Growl growl)
    {
        if (GetShowMode(panel) == GrowlShowMode.Prepend)
        {
            panel.Children.Insert(0, growl);
        }
        else
        {
            panel.Children.Add(growl);
        }
    }

    private static void ShowGlobal(GrowlInfo growlInfo)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (GrowlWindow == null)
            {
                GrowlWindow = new GrowlWindow();
                GrowlWindow.Show();
                InitGrowlPanel(GrowlWindow.GrowlPanel);
            }

            GrowlWindow.UpdatePosition(GetTransitionMode(GetMainWindow() ?? GrowlWindow));
            GrowlWindow.Show();

            var ctl = CreateGrowl(growlInfo);

            ShowInternal(GrowlWindow!.GrowlPanel, ctl);
        });
    }

    /// <summary>
    ///     显示信息
    /// </summary>
    private static void Show(GrowlInfo growlInfo)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var ctl = CreateGrowl(growlInfo);

            if (!string.IsNullOrEmpty(growlInfo.Token))
            {
                if (TokenManager.TryGetControl(growlInfo.Token, out var panel) && panel != null)
                {
                    ShowInternal(panel, ctl);
                }
            }
            else
            {
                // GrowlPanel is null, we create it automatically
                GrowlPanel ??= CreateDefaultPanel();
                if (GrowlPanel == null)
                {
                    return;
                }

                ShowInternal(GrowlPanel, ctl);

                var transitionMode = GetTransitionMode(GrowlPanel);
                GrowlPanel.VerticalAlignment = GetPanelVerticalAlignment(transitionMode);
                GrowlPanel.HorizontalAlignment = GetPanelHorizontalAlignment(transitionMode);
                GrowlPanel.SetValue(ReversibleStackPanel.ReverseOrderProperty,
                    transitionMode is TransitionMode.Bottom2Top or TransitionMode.Bottom2TopWithFade);
            }
        });
    }

    private static Growl CreateGrowl(GrowlInfo growlInfo) => new()
    {
        Message = growlInfo.Message,
        Time = DateTime.Now,
        Icon = ResolveIcon(growlInfo),
        IconBrush = ResolveIconBrush(growlInfo),
        _showCloseButton = growlInfo.ShowCloseButton,
        ActionBeforeClose = growlInfo.ActionBeforeClose,
        _staysOpen = growlInfo.StaysOpen,
        ShowDateTime = growlInfo.ShowDateTime,
        ConfirmStr = growlInfo.ConfirmStr,
        CancelStr = growlInfo.CancelStr,
        Type = growlInfo.Type,
        _waitTime = Math.Max(growlInfo.WaitTime, MinWaitTime)
    };

    private static Geometry? ResolveIcon(GrowlInfo growlInfo) =>
        string.IsNullOrEmpty(growlInfo.IconKey)
            ? growlInfo.Icon
            : ResourceHelper.GetResource<Geometry>(growlInfo.IconKey) ?? growlInfo.Icon;

    private static IBrush? ResolveIconBrush(GrowlInfo growlInfo) =>
        string.IsNullOrEmpty(growlInfo.IconBrushKey)
            ? growlInfo.IconBrush
            : ResourceHelper.GetResource<IBrush>(growlInfo.IconBrushKey) ?? growlInfo.IconBrush;

    private static Panel? CreateDefaultPanel()
    {
        var window = GetActiveWindow();
        if (window == null)
        {
            return null;
        }

        window.Closed += (_, _) => Clear(GrowlPanel);
        var overlay = OverlayLayer.GetOverlayLayer(window);
        if (overlay == null)
        {
            return null;
        }

        var panel = new ReversibleStackPanel();
        InitGrowlPanel(panel);
        SetIsCreatedAutomatically(panel, true);

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            Content = panel
        };

        overlay.Children.Add(scrollViewer);

        return panel;
    }

    private static void RemoveDefaultPanel(Panel panel)
    {
        var element = GetActiveWindow();
        if (element == null)
        {
            return;
        }

        var overlay = OverlayLayer.GetOverlayLayer(element);
        var adorner = panel.GetVisualParent() as Control;
        overlay?.Children.Remove(adorner!);
    }

    private static void InitGrowlInfo(GrowlInfo growlInfo, InfoType infoType)
    {
        if (growlInfo == null) throw new ArgumentNullException(nameof(growlInfo));
        growlInfo.Type = infoType;

        switch (infoType)
        {
            case InfoType.Success:
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.SuccessGeometry;
                    growlInfo.IconBrushKey = ResourceToken.SuccessBrush;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.SuccessGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.SuccessBrush;
                }

                break;
            case InfoType.Info:
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.InfoGeometry;
                    growlInfo.IconBrushKey = ResourceToken.InfoBrush;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.InfoGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.InfoBrush;
                }

                break;
            case InfoType.Warning:
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.WarningGeometry;
                    growlInfo.IconBrushKey = ResourceToken.WarningBrush;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.WarningGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.WarningBrush;
                }

                break;
            case InfoType.Error:
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.ErrorGeometry;
                    growlInfo.IconBrushKey = ResourceToken.DangerBrush;
                    growlInfo.StaysOpen = true;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.ErrorGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.DangerBrush;
                }

                break;
            case InfoType.Fatal:
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.FatalGeometry;
                    growlInfo.IconBrushKey = ResourceToken.PrimaryTextBrush;
                    growlInfo.StaysOpen = true;
                    growlInfo.ShowCloseButton = false;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.FatalGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.PrimaryTextBrush;
                }

                break;
            case InfoType.Ask:
                growlInfo.StaysOpen = true;
                growlInfo.ShowCloseButton = false;
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.AskGeometry;
                    growlInfo.IconBrushKey = ResourceToken.AccentBrush;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.AskGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.AccentBrush;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(infoType), infoType, null);
        }
    }

    /// <summary>
    ///     成功
    /// </summary>
    public static void Success(string message, string token = "") => Success(new GrowlInfo
    {
        Message = message,
        Token = token
    });

    public static void Success(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Success);
        Show(growlInfo);
    }

    public static void SuccessGlobal(string message) => SuccessGlobal(new GrowlInfo
    {
        Message = message
    });

    public static void SuccessGlobal(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Success);
        ShowGlobal(growlInfo);
    }

    public static void Info(string message, string token = "") => Info(new GrowlInfo
    {
        Message = message,
        Token = token
    });

    public static void Info(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Info);
        Show(growlInfo);
    }

    public static void InfoGlobal(string message) => InfoGlobal(new GrowlInfo
    {
        Message = message
    });

    public static void InfoGlobal(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Info);
        ShowGlobal(growlInfo);
    }

    public static void Warning(string message, string token = "") => Warning(new GrowlInfo
    {
        Message = message,
        Token = token
    });

    public static void Warning(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Warning);
        Show(growlInfo);
    }

    public static void WarningGlobal(string message) => WarningGlobal(new GrowlInfo
    {
        Message = message
    });

    public static void WarningGlobal(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Warning);
        ShowGlobal(growlInfo);
    }

    public static void Error(string message, string token = "") => Error(new GrowlInfo
    {
        Message = message,
        Token = token
    });

    public static void Error(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Error);
        Show(growlInfo);
    }

    public static void ErrorGlobal(string message) => ErrorGlobal(new GrowlInfo
    {
        Message = message
    });

    public static void ErrorGlobal(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Error);
        ShowGlobal(growlInfo);
    }

    public static void Fatal(string message, string token = "") => Fatal(new GrowlInfo
    {
        Message = message,
        Token = token
    });

    public static void Fatal(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Fatal);
        Show(growlInfo);
    }

    public static void FatalGlobal(string message) => FatalGlobal(new GrowlInfo
    {
        Message = message
    });

    public static void FatalGlobal(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Fatal);
        ShowGlobal(growlInfo);
    }

    public static void Ask(string message, Func<bool, bool> actionBeforeClose, string token = "") => Ask(new GrowlInfo
    {
        Message = message,
        ActionBeforeClose = actionBeforeClose,
        Token = token
    });

    public static void Ask(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Ask);
        Show(growlInfo);
    }

    public static void AskGlobal(string message, Func<bool, bool> actionBeforeClose) => AskGlobal(new GrowlInfo
    {
        Message = message,
        ActionBeforeClose = actionBeforeClose
    });

    public static void AskGlobal(GrowlInfo growlInfo)
    {
        InitGrowlInfo(growlInfo, InfoType.Ask);
        ShowGlobal(growlInfo);
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e) => Close(false);

    /// <summary>
    ///     关闭
    /// </summary>
    private async void Close(bool invokeParam, bool isClear = false)
    {
        if (!isClear && ActionBeforeClose?.Invoke(invokeParam) == false)
        {
            return;
        }

        _timerClose?.Stop();
        ZIndex = int.MinValue;
        await StartTransitionAsync(true);
        OnCloseTransitionCompleted();
    }

    private void OnCloseTransitionCompleted()
    {
        if (Parent is not Panel panel)
        {
            return;
        }

        panel.Children.Remove(this);

        if (GrowlWindow != null)
        {
            if (GrowlWindow.GrowlPanel is not { Children.Count: 0 })
            {
                return;
            }

            GrowlWindow.Close();
            GrowlWindow = null;
        }
        else
        {
            if (GrowlPanel is not { Children.Count: 0 } || !GetIsCreatedAutomatically(GrowlPanel))
            {
                return;
            }

            // If the count of children is zero, we need to remove the panel, provided that the panel was created automatically
            RemoveDefaultPanel(GrowlPanel);
            GrowlPanel = null;
        }
    }

    /// <summary>
    ///     清除
    /// </summary>
    public static void Clear(string token = "")
    {
        if (!string.IsNullOrEmpty(token))
        {
            if (TokenManager.TryGetControl(token, out var panel))
            {
                Clear(panel);
            }
        }
        else
        {
            Clear(GrowlPanel);
        }
    }

    /// <summary>
    ///     清除
    /// </summary>
    private static void Clear(Panel? panel) => panel?.Children.Clear();

    /// <summary>
    ///     清除
    /// </summary>
    public static void ClearGlobal()
    {
        if (GrowlWindow == null) return;
        Clear(GrowlWindow.GrowlPanel);
        GrowlWindow.Close();
        GrowlWindow = null;
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e) => Close(false);

    private void ButtonOk_OnClick(object? sender, RoutedEventArgs e) => Close(true);

    private void StartTransition(bool isClose)
    {
        _transitionCts?.Cancel();
        _transitionCts = new CancellationTokenSource();
        _ = StartTransitionAsync(isClose, _transitionCts.Token);
    }

    private async Task StartTransitionAsync(bool isClose, CancellationToken token = default)
    {
        if (_gridMain == null)
        {
            return;
        }

        var transitionMode = GetTransitionMode(this);
        var transformLength = GetTransformLength(isClose, transitionMode);
        var orientation = GetOrientation(transitionMode);

        var translate = new TranslateTransform();
        _gridMain.RenderTransform = translate;

        if (orientation == Orientation.Horizontal)
        {
            translate.X = isClose ? 0 : transformLength;
        }
        else
        {
            translate.Y = isClose ? 0 : transformLength;
        }

        if (transitionMode is TransitionMode.Fade)
        {
            _gridMain.Opacity = isClose ? 1 : 0;
        }
        else if (transitionMode is TransitionMode.Right2LeftWithFade or TransitionMode.Left2RightWithFade
                 or TransitionMode.Bottom2TopWithFade or TransitionMode.Top2BottomWithFade)
        {
            _gridMain.Opacity = isClose ? 1 : 0;
        }

        var translateAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(TransitionMilliseconds),
            Easing = new CubicEaseInOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, orientation == Orientation.Horizontal ? transformLength : 0d),
                        new Setter(TranslateTransform.YProperty, orientation == Orientation.Vertical ? transformLength : 0d)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, 0d),
                        new Setter(TranslateTransform.YProperty, 0d)
                    }
                }
            }
        };

        var opacityAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(TransitionMilliseconds),
            Easing = new CubicEaseInOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(Visual.OpacityProperty, GetStartOpacity(isClose, transitionMode)) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(Visual.OpacityProperty, GetEndOpacity(isClose, transitionMode)) }
                }
            }
        };

        try
        {
            var tasks = new List<Task>
            {
                translateAnimation.RunAsync(translate, token),
                opacityAnimation.RunAsync(_gridMain, token)
            };
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static double GetStartOpacity(bool isClose, TransitionMode transitionMode)
    {
        if (transitionMode is TransitionMode.Right2Left or TransitionMode.Left2Right
            or TransitionMode.Bottom2Top or TransitionMode.Top2Bottom)
        {
            return 1d;
        }

        return isClose ? 1d : 0d;
    }

    private static double GetEndOpacity(bool isClose, TransitionMode transitionMode)
    {
        if (transitionMode is TransitionMode.Right2Left or TransitionMode.Left2Right
            or TransitionMode.Bottom2Top or TransitionMode.Top2Bottom)
        {
            return 1d;
        }

        return isClose ? 0d : 1d;
    }

    private double GetTransformLength(bool isClose, TransitionMode transitionMode)
    {
        var length = transitionMode switch
        {
            TransitionMode.Right2Left or TransitionMode.Right2LeftWithFade => Bounds.Width,
            TransitionMode.Left2Right or TransitionMode.Left2RightWithFade => -Bounds.Width,
            TransitionMode.Bottom2Top or TransitionMode.Bottom2TopWithFade => Bounds.Height,
            TransitionMode.Top2Bottom or TransitionMode.Top2BottomWithFade => -Bounds.Height,
            _ => Bounds.Width
        };

        return isClose ? -length : length;
    }

    private static Orientation? GetOrientation(TransitionMode transitionMode)
    {
        return transitionMode switch
        {
            TransitionMode.Right2Left or TransitionMode.Right2LeftWithFade or TransitionMode.Left2Right
                or TransitionMode.Left2RightWithFade => Orientation.Horizontal,
            TransitionMode.Bottom2Top or TransitionMode.Bottom2TopWithFade or TransitionMode.Top2Bottom
                or TransitionMode.Top2BottomWithFade => Orientation.Vertical,
            _ => Orientation.Horizontal
        };
    }

    internal static VerticalAlignment GetPanelVerticalAlignment(TransitionMode transitionMode) =>
        VerticalAlignment.Stretch;

    internal static HorizontalAlignment GetPanelHorizontalAlignment(TransitionMode transitionMode) =>
        transitionMode switch
        {
            TransitionMode.Right2Left or TransitionMode.Right2LeftWithFade => HorizontalAlignment.Right,
            TransitionMode.Left2Right or TransitionMode.Left2RightWithFade => HorizontalAlignment.Left,
            TransitionMode.Bottom2Top or TransitionMode.Bottom2TopWithFade or TransitionMode.Top2Bottom
                or TransitionMode.Top2BottomWithFade => HorizontalAlignment.Center,
            _ => HorizontalAlignment.Right
        };

    private static Avalonia.Controls.Window? GetActiveWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.Windows.FirstOrDefault(w => w.IsActive) ?? desktop.MainWindow;
        }

        return null;
    }

    private static Avalonia.Controls.Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }
}
