using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using HandyControl.Data;
using HandyControl.Tools;

namespace HandyControl.Controls;

/// <summary>
///     抽屉
/// </summary>
public class Drawer : Control
{
    private const double TransitionMilliseconds = 200;

    private CancellationTokenSource? _animationCts;
    private Panel? _overlayHost;
    private Border? _maskElement;
    private ContentControl? _animationControl;
    private TranslateTransform? _translateTransform;
    private Control? _contentElement;
    private double _animationLength;
    private bool _animationLengthInitialized;

    static Drawer()
    {
        IsOpenProperty.Changed.AddClassHandler<Drawer>(OnIsOpenChanged);
    }

    public Drawer()
    {
        CloseCommand = new CloseDrawerCommand(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (IsOpen)
        {
            OnIsOpenChanged(true);
        }
    }

    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<Drawer, RoutedEventArgs>("Opened", RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<Drawer, RoutedEventArgs>("Closed", RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsOpen), false, defaultBindingMode: BindingMode.TwoWay);

    private static void OnIsOpenChanged(Drawer d, AvaloniaPropertyChangedEventArgs e) =>
        d.OnIsOpenChanged(e.GetNewValue<bool>());

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly StyledProperty<bool> MaskCanCloseProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(MaskCanClose), true);

    public bool MaskCanClose
    {
        get => GetValue(MaskCanCloseProperty);
        set => SetValue(MaskCanCloseProperty, value);
    }

    public static readonly StyledProperty<bool> ShowMaskProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(ShowMask), true);

    public bool ShowMask
    {
        get => GetValue(ShowMaskProperty);
        set => SetValue(ShowMaskProperty, value);
    }

    public static readonly StyledProperty<Dock> DockProperty =
        AvaloniaProperty.Register<Drawer, Dock>(nameof(Dock));

    public Dock Dock
    {
        get => GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    public static readonly StyledProperty<DrawerShowMode> ShowModeProperty =
        AvaloniaProperty.Register<Drawer, DrawerShowMode>(nameof(ShowMode));

    public DrawerShowMode ShowMode
    {
        get => GetValue(ShowModeProperty);
        set => SetValue(ShowModeProperty, value);
    }

    public static readonly StyledProperty<IBrush?> MaskBrushProperty =
        AvaloniaProperty.Register<Drawer, IBrush?>(nameof(MaskBrush));

    public IBrush? MaskBrush
    {
        get => GetValue(MaskBrushProperty);
        set => SetValue(MaskBrushProperty, value);
    }

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Drawer, object?>(nameof(Content));

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public ICommand CloseCommand { get; }

    private void CreateContainer()
    {
        _translateTransform = new TranslateTransform();
        _animationControl = new ContentControl
        {
            Content = Content,
            RenderTransform = _translateTransform,
            DataContext = this
        };

        _animationLengthInitialized = false;
    }

    private void UpdateContainer()
    {
        if (_animationControl == null)
        {
            return;
        }

        if (!_animationLengthInitialized)
        {
            _animationControl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var size = _animationControl.DesiredSize;

            switch (Dock)
            {
                case Dock.Left:
                    _animationControl.HorizontalAlignment = HorizontalAlignment.Left;
                    _animationControl.VerticalAlignment = VerticalAlignment.Stretch;
                    _animationLength = -size.Width;
                    break;
                case Dock.Top:
                    _animationControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _animationControl.VerticalAlignment = VerticalAlignment.Top;
                    _animationLength = -size.Height;
                    break;
                case Dock.Right:
                    _animationControl.HorizontalAlignment = HorizontalAlignment.Right;
                    _animationControl.VerticalAlignment = VerticalAlignment.Stretch;
                    _animationLength = size.Width;
                    break;
                case Dock.Bottom:
                    _animationControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _animationControl.VerticalAlignment = VerticalAlignment.Bottom;
                    _animationLength = size.Height;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _animationLengthInitialized = true;
        }
    }

    private void OnIsOpenChanged(bool isOpen)
    {
        if (Content == null || Design.IsDesignMode)
        {
            return;
        }

        var container = VisualHelper.GetParent<DrawerContainer>(this);
        Panel? overlayHost;

        if (container != null)
        {
            overlayHost = container.OverlayPanel;
            _contentElement = container.Content as Control;
        }
        else
        {
            var window = TopLevel.GetTopLevel(this) as Window;
            overlayHost = window == null ? null : OverlayLayer.GetOverlayLayer(window);
            _contentElement = window?.Content as Control;
        }

        if (overlayHost == null)
        {
            return;
        }

        _overlayHost = overlayHost;

        if (_animationControl == null)
        {
            CreateContainer();
        }

        _animationControl!.Content = Content;

        if (isOpen)
        {
            if (_maskElement == null && ShowMask)
            {
                _maskElement = new Border
                {
                    Background = MaskBrush,
                    Opacity = 0,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                _maskElement.PointerPressed += MaskElement_PointerPressed;
            }

            _animationControl.Opacity = 1;
            UpdateContainer();

            if (_maskElement != null && !_overlayHost.Children.Contains(_maskElement))
            {
                _overlayHost.Children.Add(_maskElement);
            }

            if (!_overlayHost.Children.Contains(_animationControl))
            {
                _overlayHost.Children.Add(_animationControl);
            }
        }

        _animationCts?.Cancel();
        _animationCts = new CancellationTokenSource();
        var token = _animationCts.Token;

        _ = AnimateAsync(isOpen, token);
    }

    private void MaskElement_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (MaskCanClose)
        {
            IsOpen = false;
        }
    }

    private async Task AnimateAsync(bool isOpen, CancellationToken token)
    {
        try
        {
            var tasks = new System.Collections.Generic.List<Task>();

            if (_maskElement != null)
            {
                var maskOpacity = CreateAnimation(Visual.OpacityProperty, isOpen ? 0d : 1d, isOpen ? 1d : 0d);
                tasks.Add(maskOpacity.RunAsync(_maskElement, token));
            }

            if (_translateTransform != null)
            {
                var prop = Dock is Dock.Left or Dock.Right
                    ? TranslateTransform.XProperty
                    : TranslateTransform.YProperty;

                var from = isOpen ? _animationLength : 0d;
                var to = isOpen ? 0d : _animationLength;

                var drawerAnimation = CreateAnimation(prop, from, to);
                tasks.Add(drawerAnimation.RunAsync(_translateTransform, token));
            }

            if (_contentElement != null && _overlayHost != null && _animationControl != null &&
                _overlayHost.Children.Contains(_animationControl))
            {
                switch (ShowMode)
                {
                    case DrawerShowMode.Push:
                        var contentTranslate = new TranslateTransform();
                        _contentElement.RenderTransform = contentTranslate;
                        var contentProp = Dock is Dock.Left or Dock.Right
                            ? TranslateTransform.XProperty
                            : TranslateTransform.YProperty;
                        var contentFrom = isOpen ? 0d : -_animationLength;
                        var contentTo = isOpen ? -_animationLength : 0d;
                        var contentAnimation = CreateAnimation(contentProp, contentFrom, contentTo);
                        tasks.Add(contentAnimation.RunAsync(contentTranslate, token));
                        break;
                    case DrawerShowMode.Press:
                        var contentScale = new ScaleTransform
                        {
                            ScaleX = isOpen ? 0.9 : 1,
                            ScaleY = isOpen ? 0.9 : 1
                        };
                        _contentElement.RenderTransform = contentScale;
                        var scaleFrom = isOpen ? 1d : 0.9;
                        var scaleTo = isOpen ? 0.9 : 1d;
                        var scaleX = CreateAnimation(ScaleTransform.ScaleXProperty, scaleFrom, scaleTo);
                        var scaleY = CreateAnimation(ScaleTransform.ScaleYProperty, scaleFrom, scaleTo);
                        tasks.Add(scaleX.RunAsync(contentScale, token));
                        tasks.Add(scaleY.RunAsync(contentScale, token));
                        break;
                }
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }

            if (isOpen)
            {
                RaiseEvent(new RoutedEventArgs(OpenedEvent));
            }
            else
            {
                if (_maskElement != null)
                {
                    _overlayHost?.Children.Remove(_maskElement);
                }

                if (_animationControl != null)
                {
                    _overlayHost?.Children.Remove(_animationControl);
                }

                if (_contentElement != null)
                {
                    _contentElement.RenderTransform = null;
                }

                RaiseEvent(new RoutedEventArgs(ClosedEvent));
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static Animation CreateAnimation(AvaloniaProperty property, double from, double to)
    {
        return new Animation
        {
            Duration = TimeSpan.FromMilliseconds(TransitionMilliseconds),
            Easing = new QuadraticEaseInOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(property, from) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(property, to) }
                }
            }
        };
    }

    private sealed class CloseDrawerCommand : ICommand
    {
        private readonly Drawer _drawer;

        public CloseDrawerCommand(Drawer drawer)
        {
            _drawer = drawer;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _drawer.IsOpen = false;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
