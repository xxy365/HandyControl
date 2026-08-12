using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;

namespace HandyControl.Controls;

public class FloatingBlock : TemplatedControl
{
    public static readonly AttachedProperty<double> ToXProperty =
        AvaloniaProperty.RegisterAttached<FloatingBlock, AvaloniaObject, double>("ToX");

    public static void SetToX(AvaloniaObject element, double value) => element.SetValue(ToXProperty, value);

    public static double GetToX(AvaloniaObject element) => element.GetValue(ToXProperty);

    public static readonly AttachedProperty<double> ToYProperty =
        AvaloniaProperty.RegisterAttached<FloatingBlock, AvaloniaObject, double>("ToY", defaultValue: -100.0);

    public static void SetToY(AvaloniaObject element, double value) => element.SetValue(ToYProperty, value);

    public static double GetToY(AvaloniaObject element) => element.GetValue(ToYProperty);

    public static readonly AttachedProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<FloatingBlock, AvaloniaObject, TimeSpan>("Duration",
            defaultValue: TimeSpan.FromSeconds(2));

    public static void SetDuration(AvaloniaObject element, TimeSpan value) => element.SetValue(DurationProperty, value);

    public static TimeSpan GetDuration(AvaloniaObject element) => element.GetValue(DurationProperty);

    public static readonly AttachedProperty<double> HorizontalOffsetProperty =
        AvaloniaProperty.RegisterAttached<FloatingBlock, AvaloniaObject, double>("HorizontalOffset");

    public static void SetHorizontalOffset(AvaloniaObject element, double value) =>
        element.SetValue(HorizontalOffsetProperty, value);

    public static double GetHorizontalOffset(AvaloniaObject element) => element.GetValue(HorizontalOffsetProperty);

    public static readonly AttachedProperty<double> VerticalOffsetProperty =
        AvaloniaProperty.RegisterAttached<FloatingBlock, AvaloniaObject, double>("VerticalOffset");

    public static void SetVerticalOffset(AvaloniaObject element, double value) =>
        element.SetValue(VerticalOffsetProperty, value);

    public static double GetVerticalOffset(AvaloniaObject element) => element.GetValue(VerticalOffsetProperty);

    public static readonly AttachedProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.RegisterAttached<FloatingBlock, AvaloniaObject, IDataTemplate?>("ContentTemplate");

    public static void SetContentTemplate(AvaloniaObject element, IDataTemplate? value) =>
        element.SetValue(ContentTemplateProperty, value);

    public static IDataTemplate? GetContentTemplate(AvaloniaObject element) => element.GetValue(ContentTemplateProperty);

    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    private static readonly AttachedProperty<bool> ReadyToFloatProperty =
        AvaloniaProperty.RegisterAttached<FloatingBlock, AvaloniaObject, bool>("ReadyToFloat");

    private static void SetReadyToFloat(AvaloniaObject element, bool value) =>
        element.SetValue(ReadyToFloatProperty, value);

    private static bool GetReadyToFloat(AvaloniaObject element) => element.GetValue(ReadyToFloatProperty);

    public static readonly AttachedProperty<object?> ContentProperty =
        AvaloniaProperty.RegisterAttached<FloatingBlock, AvaloniaObject, object?>("Content");

    public static void SetContent(AvaloniaObject element, object? value) => element.SetValue(ContentProperty, value);

    public static object? GetContent(AvaloniaObject element) => element.GetValue(ContentProperty);

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    static FloatingBlock()
    {
        ContentTemplateProperty.Changed.AddClassHandler<AvaloniaObject>(OnDataChanged);
        ContentProperty.Changed.AddClassHandler<AvaloniaObject>(OnDataChanged);
    }

    private static void OnDataChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not Control target)
        {
            return;
        }

        target.PointerPressed -= Target_PointerPressed;
        target.PointerReleased -= Target_PointerReleased;

        if (e.NewValue != null)
        {
            target.PointerPressed += Target_PointerPressed;
            target.PointerReleased += Target_PointerReleased;
        }
    }

    private static void Target_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is InputElement input && e.GetCurrentPoint(input).Properties.IsLeftButtonPressed)
        {
            SetReadyToFloat(input, true);
        }
    }

    private static void Target_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Control element && GetReadyToFloat(element))
        {
            var window = TopLevel.GetTopLevel(element) as Window;
            if (window != null)
            {
                var layer = OverlayLayer.GetOverlayLayer(window);
                if (layer != null)
                {
                    var block = CreateBlock(element, layer, e);
                    layer.Children.Add(block);
                }
            }

            SetReadyToFloat(element, false);
        }
    }

    private static FloatingBlock CreateBlock(Control element, Panel layer, PointerReleasedEventArgs e)
    {
        var p = e.GetPosition(layer);
        var transform = new TranslateTransform
        {
            X = p.X + GetHorizontalOffset(element),
            Y = p.Y + GetVerticalOffset(element)
        };

        var block = new FloatingBlock
        {
            Content = GetContent(element),
            ContentTemplate = GetContentTemplate(element),
            IsHitTestVisible = false,
            RenderTransform = new TransformGroup
            {
                Children =
                {
                    transform
                }
            }
        };

        var milliseconds = GetDuration(element).TotalMilliseconds;

        var animationX = CreateAnimation(TranslateTransform.XProperty, transform.X, GetToX(element) + transform.X,
            milliseconds);
        var animationY = CreateAnimation(TranslateTransform.YProperty, transform.Y, GetToY(element) + transform.Y,
            milliseconds);
        var animationOpacity = CreateAnimation(Visual.OpacityProperty, 1, 0, milliseconds);

        _ = AnimateAsync(transform, animationX);
        _ = AnimateAsync(transform, animationY);
        _ = AnimateAsync(block, animationOpacity).ContinueWith(_ => layer.Children.Remove(block),
            TaskScheduler.FromCurrentSynchronizationContext());

        return block;
    }

    private static async Task AnimateAsync(Avalonia.Animation.Animatable target, Animation animation)
    {
        try
        {
            await animation.RunAsync(target);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static Animation CreateAnimation(AvaloniaProperty property, double from, double to, double milliseconds)
    {
        return new Animation
        {
            Duration = TimeSpan.FromMilliseconds(milliseconds),
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
}
