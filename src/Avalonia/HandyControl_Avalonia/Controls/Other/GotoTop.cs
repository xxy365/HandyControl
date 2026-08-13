using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace HandyControl.Controls;

public class GotoTop : Button
{
    public static readonly StyledProperty<Control?> TargetProperty =
        AvaloniaProperty.Register<GotoTop, Control?>(nameof(Target));

    public static readonly StyledProperty<bool> AnimatedProperty =
        AvaloniaProperty.Register<GotoTop, bool>(nameof(Animated), true);

    public static readonly StyledProperty<double> AnimationTimeProperty =
        AvaloniaProperty.Register<GotoTop, double>(nameof(AnimationTime), 200d);

    public static readonly StyledProperty<double> HidingHeightProperty =
        AvaloniaProperty.Register<GotoTop, double>(nameof(HidingHeight));

    public static readonly StyledProperty<bool> AutoHidingProperty =
        AvaloniaProperty.Register<GotoTop, bool>(nameof(AutoHiding), true);

    private ScrollViewer? _scrollViewer;
    private CancellationTokenSource? _scrollCts;
    private double _animationStartOffset;

    static GotoTop()
    {
        TargetProperty.Changed.AddClassHandler<GotoTop>((gotoTop, e) => gotoTop.CreateGotoAction(e.GetNewValue<Control?>()));
        AutoHidingProperty.Changed.AddClassHandler<GotoTop>((gotoTop, _) => gotoTop.UpdateVisibility());
        HidingHeightProperty.Changed.AddClassHandler<GotoTop>((gotoTop, _) => gotoTop.UpdateVisibility());
    }

    public GotoTop()
    {
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        CreateGotoAction(Target);
    }

    public Control? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public bool Animated
    {
        get => GetValue(AnimatedProperty);
        set => SetValue(AnimatedProperty, value);
    }

    public double AnimationTime
    {
        get => GetValue(AnimationTimeProperty);
        set => SetValue(AnimationTimeProperty, value);
    }

    public double HidingHeight
    {
        get => GetValue(HidingHeightProperty);
        set => SetValue(HidingHeightProperty, value);
    }

    public bool AutoHiding
    {
        get => GetValue(AutoHidingProperty);
        set => SetValue(AutoHidingProperty, value);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged -= ScrollViewerOnScrollChanged;
        }

        if (_scrollCts != null)
        {
            _scrollCts.Cancel();
        }
    }

    public virtual void CreateGotoAction(Control? obj)
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged -= ScrollViewerOnScrollChanged;
        }

        _scrollViewer = null;
        var target = obj ?? this;

        if (target is ScrollViewer directScrollViewer)
        {
            _scrollViewer = directScrollViewer;
        }
        else
        {
            _scrollViewer = target.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        }

        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += ScrollViewerOnScrollChanged;
        }

        UpdateVisibility();
    }

    private void ScrollViewerOnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (!AutoHiding)
        {
            IsVisible = true;
            return;
        }

        IsVisible = (_scrollViewer?.Offset.Y ?? 0d) >= HidingHeight;
    }

    protected override void OnClick()
    {
        base.OnClick();

        if (_scrollViewer == null)
        {
            return;
        }

        if (!Animated || AnimationTime <= 0)
        {
            _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, 0);
            return;
        }

        StartAnimatedScroll();
    }

    private void StartAnimatedScroll()
    {
        if (_scrollViewer == null)
        {
            return;
        }

        _animationStartOffset = _scrollViewer.Offset.Y;
        if (_animationStartOffset <= 0)
        {
            return;
        }

        _scrollCts?.Cancel();
        _scrollCts = new CancellationTokenSource();
        var token = _scrollCts.Token;

        var start = _scrollViewer.Offset;
        var end = new Vector(_scrollViewer.Offset.X, 0d);

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(AnimationTime),
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(ScrollViewer.OffsetProperty, start) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(ScrollViewer.OffsetProperty, end) }
                }
            }
        };

        _ = animation.RunAsync(_scrollViewer, token);
    }
}
