using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using HandyControl.Data;
using HandyControl.Tools;

namespace HandyControl.Controls;

public class Poptip : AdornerElement
{
    private readonly Popup _popup;

    private DispatcherTimer? _openTimer;

    public Poptip()
    {
        _popup = new Popup
        {
            Child = this,
            Placement = PlacementMode.AnchorAndGravity,
            PlacementAnchor = PopupAnchor.TopLeft,
            PlacementGravity = PopupGravity.TopLeft,
            DataContext = this
        };
    }

    public static readonly AttachedProperty<HitMode> HitModeProperty =
        AvaloniaProperty.RegisterAttached<Poptip, AvaloniaObject, HitMode>("HitMode", HitMode.Hover);

    public static void SetHitMode(AvaloniaObject element, HitMode value) =>
        element.SetValue(HitModeProperty, value);

    public static HitMode GetHitMode(AvaloniaObject element) =>
        (HitMode) element.GetValue(HitModeProperty);

    public HitMode HitMode
    {
        get => (HitMode) GetValue(HitModeProperty);
        set => SetValue(HitModeProperty, value);
    }

    public static readonly AttachedProperty<object?> ContentProperty =
        AvaloniaProperty.RegisterAttached<Poptip, AvaloniaObject, object?>("Content");

    public static void SetContent(AvaloniaObject element, object? value) =>
        element.SetValue(ContentProperty, value);

    public static object? GetContent(AvaloniaObject element) =>
        (object?) element.GetValue(ContentProperty);

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<Poptip, IDataTemplate?>(nameof(ContentTemplate));

    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    public static readonly AttachedProperty<double> VerticalOffsetProperty =
        AvaloniaProperty.RegisterAttached<Poptip, AvaloniaObject, double>("VerticalOffset");

    public static void SetVerticalOffset(AvaloniaObject element, double value) =>
        element.SetValue(VerticalOffsetProperty, value);

    public static double GetVerticalOffset(AvaloniaObject element) =>
        (double) element.GetValue(VerticalOffsetProperty);

    public double VerticalOffset
    {
        get => (double) GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    public static readonly AttachedProperty<double> HorizontalOffsetProperty =
        AvaloniaProperty.RegisterAttached<Poptip, AvaloniaObject, double>("HorizontalOffset");

    public static void SetHorizontalOffset(AvaloniaObject element, double value) =>
        element.SetValue(HorizontalOffsetProperty, value);

    public static double GetHorizontalOffset(AvaloniaObject element) =>
        (double) element.GetValue(HorizontalOffsetProperty);

    public double HorizontalOffset
    {
        get => (double) GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    public static readonly AttachedProperty<PlacementType> PlacementTypeProperty =
        AvaloniaProperty.RegisterAttached<Poptip, AvaloniaObject, PlacementType>("PlacementType",
            PlacementType.Top);

    public static void SetPlacementType(AvaloniaObject element, PlacementType value) =>
        element.SetValue(PlacementTypeProperty, value);

    public static PlacementType GetPlacementType(AvaloniaObject element) =>
        (PlacementType) element.GetValue(PlacementTypeProperty);

    public static void SetPlacement(AvaloniaObject element, PlacementType value) =>
        element.SetValue(PlacementTypeProperty, value);

    public static PlacementType GetPlacement(AvaloniaObject element) =>
        (PlacementType) element.GetValue(PlacementTypeProperty);

    public PlacementType PlacementType
    {
        get => (PlacementType) GetValue(PlacementTypeProperty);
        set => SetValue(PlacementTypeProperty, value);
    }

    public static readonly AttachedProperty<bool> IsOpenProperty =
        AvaloniaProperty.RegisterAttached<Poptip, AvaloniaObject, bool>("IsOpen");

    static Poptip()
    {
        ContentProperty.Changed.AddClassHandler<AvaloniaObject>(OnContentChanged);
        IsOpenProperty.Changed.AddClassHandler<AvaloniaObject>(OnIsOpenChanged);
    }

    private static void OnContentChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is Poptip) return;
        if (GetInstance(d) == null)
        {
            SetInstance(d, Default);
            SetIsInstance(d, false);
        }
    }

    private static void OnIsOpenChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is Poptip poptip)
        {
            poptip.SwitchPoptip((bool) e.NewValue!);
        }
        else
        {
            (GetInstance(d) as Poptip)?.SwitchPoptip((bool) e.NewValue!);
        }
    }

    public static void SetIsOpen(AvaloniaObject element, bool value) =>
        element.SetValue(IsOpenProperty, value);

    public static bool GetIsOpen(AvaloniaObject element) =>
        (bool) element.GetValue(IsOpenProperty);

    public bool IsOpen
    {
        get => (bool) GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly StyledProperty<double> DelayProperty =
        AvaloniaProperty.Register<Poptip, double>(nameof(Delay), 1000.0);

    public double Delay
    {
        get => (double) GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    public static Poptip Default => new();

    protected sealed override void OnTargetChanged(Control? element, bool isNew)
    {
        base.OnTargetChanged(element, isNew);

        if (element == null) return;

        if (!isNew)
        {
            element.PointerEntered -= Element_PointerEntered;
            element.PointerExited -= Element_PointerExited;
            element.GotFocus -= Element_GotFocus;
            element.LostFocus -= Element_LostFocus;
            ElementTarget = null;
        }
        else
        {
            element.PointerEntered += Element_PointerEntered;
            element.PointerExited += Element_PointerExited;
            element.GotFocus += Element_GotFocus;
            element.LostFocus += Element_LostFocus;
            ElementTarget = element;
            _popup.PlacementTarget = ElementTarget;
        }
    }

    protected override void Dispose() => SwitchPoptip(false);

    private void UpdateLocation()
    {
        if (Target == null) return;

        var targetSize = Target.Bounds.Size;

        Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var size = DesiredSize;

        var width = size.Width;
        var height = size.Height;

        var offsetX = 0.0;
        var offsetY = 0.0;

        var poptip = (Poptip) GetInstance(Target)!;
        var popupPlacement = poptip.PlacementType;
        var popupOffsetX = poptip.HorizontalOffset;
        var popupOffsetY = poptip.VerticalOffset;

        switch (popupPlacement)
        {
            case PlacementType.LeftTop:
                break;
            case PlacementType.Left:
                offsetY = -(height - targetSize.Height) * 0.5;
                break;
            case PlacementType.LeftBottom:
                offsetY = -(height - targetSize.Height);
                break;
            case PlacementType.TopLeft:
                offsetX = width;
                offsetY = -height;
                break;
            case PlacementType.Top:
                offsetX = (width + targetSize.Width) * 0.5;
                offsetY = -height;
                break;
            case PlacementType.TopRight:
                offsetX = targetSize.Width;
                offsetY = -height;
                break;
            case PlacementType.RightTop:
                offsetX = width + targetSize.Width;
                break;
            case PlacementType.Right:
                offsetX = width + targetSize.Width;
                offsetY = -(height - targetSize.Height) * 0.5;
                break;
            case PlacementType.RightBottom:
                offsetX = width + targetSize.Width;
                offsetY = -(height - targetSize.Height);
                break;
            case PlacementType.BottomLeft:
                offsetX = width;
                offsetY = targetSize.Height;
                break;
            case PlacementType.Bottom:
                offsetX = (width + targetSize.Width) * 0.5;
                offsetY = targetSize.Height;
                break;
            case PlacementType.BottomRight:
                offsetX = targetSize.Width;
                offsetY = targetSize.Height;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _popup.HorizontalOffset = offsetX + popupOffsetX;
        _popup.VerticalOffset = offsetY + popupOffsetY;
    }

    private void SwitchPoptip(bool isShow)
    {
        if (Target == null) return;

        if (isShow)
        {
            if (!GetIsInstance(Target))
            {
                SetCurrentValue(ContentProperty, GetContent(Target));
                SetCurrentValue(PlacementTypeProperty, GetPlacement(Target));
                SetCurrentValue(HitModeProperty, GetHitMode(Target));
                SetCurrentValue(HorizontalOffsetProperty, GetHorizontalOffset(Target));
                SetCurrentValue(VerticalOffsetProperty, GetVerticalOffset(Target));
                SetCurrentValue(IsOpenProperty, GetIsOpen(Target));
            }

            _popup.PlacementTarget = Target;
            UpdateLocation();
        }

        ResetTimer();

        var delay = Delay;
        if (!isShow || HitMode != HitMode.Hover || MathHelper.IsVerySmall(delay))
        {
            _popup.IsOpen = isShow;
            Target.SetCurrentValue(IsOpenProperty, isShow);
        }
        else
        {
            _openTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(delay)
            };

            _openTimer.Tick += OpenTimer_Tick;
            _openTimer.Start();
        }
    }

    private void ResetTimer()
    {
        if (_openTimer != null)
        {
            _openTimer.Stop();
            _openTimer = null;
        }
    }

    private void OpenTimer_Tick(object? sender, EventArgs e)
    {
        if (Target == null) return;

        _popup.IsOpen = true;
        Target.SetCurrentValue(IsOpenProperty, true);

        ResetTimer();
    }

    private void Element_PointerEntered(object? sender, PointerEventArgs e)
    {
        var hitMode = GetIsInstance(Target!) ? HitMode : GetHitMode(Target!);
        if (hitMode != HitMode.Hover) return;

        SwitchPoptip(true);
    }

    private void Element_PointerExited(object? sender, PointerEventArgs e)
    {
        var hitMode = GetIsInstance(Target!) ? HitMode : GetHitMode(Target!);
        if (hitMode != HitMode.Hover) return;

        SwitchPoptip(false);
    }

    private void Element_GotFocus(object? sender, RoutedEventArgs e)
    {
        var hitMode = GetIsInstance(Target!) ? HitMode : GetHitMode(Target!);
        if (hitMode != HitMode.Focus) return;

        SwitchPoptip(true);
    }

    private void Element_LostFocus(object? sender, RoutedEventArgs e)
    {
        var hitMode = GetIsInstance(Target!) ? HitMode : GetHitMode(Target!);
        if (hitMode != HitMode.Focus) return;

        SwitchPoptip(false);
    }
}
