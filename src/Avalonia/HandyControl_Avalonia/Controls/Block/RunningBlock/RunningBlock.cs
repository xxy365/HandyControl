using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using HandyControl.Data;
using HandyControl.Tools;

namespace HandyControl.Controls;

[TemplatePart(ElementContent, typeof(Control))]
[TemplatePart(ElementPanel, typeof(Panel))]
public class RunningBlock : ContentControl
{
    private const string ElementContent = "PART_ContentElement";

    private const string ElementPanel = "PART_Panel";

    private CancellationTokenSource? _animationCts;

    private Control? _elementContent;

    private Panel? _elementPanel;

    private TranslateTransform? _translateTransform;

    public static readonly StyledProperty<bool> RunawayProperty =
        AvaloniaProperty.Register<RunningBlock, bool>(nameof(Runaway), true);

    public bool Runaway
    {
        get => GetValue(RunawayProperty);
        set => SetValue(RunawayProperty, value);
    }

    public static readonly StyledProperty<bool> AutoRunProperty =
        AvaloniaProperty.Register<RunningBlock, bool>(nameof(AutoRun));

    public bool AutoRun
    {
        get => GetValue(AutoRunProperty);
        set => SetValue(AutoRunProperty, value);
    }

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<RunningBlock, Orientation>(nameof(Orientation));

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<RunningBlock, TimeSpan>(nameof(Duration),
            defaultValue: TimeSpan.FromSeconds(5));

    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly StyledProperty<double> SpeedProperty =
        AvaloniaProperty.Register<RunningBlock, double>(nameof(Speed), defaultValue: double.NaN);

    public double Speed
    {
        get => GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public static readonly StyledProperty<bool> IsRunningProperty =
        AvaloniaProperty.Register<RunningBlock, bool>(nameof(IsRunning), true);

    public bool IsRunning
    {
        get => GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public static readonly StyledProperty<bool> AutoReverseProperty =
        AvaloniaProperty.Register<RunningBlock, bool>(nameof(AutoReverse));

    public bool AutoReverse
    {
        get => GetValue(AutoReverseProperty);
        set => SetValue(AutoReverseProperty, value);
    }

    public static readonly StyledProperty<RunningDirection> RunningDirectionProperty =
        AvaloniaProperty.Register<RunningBlock, RunningDirection>(nameof(RunningDirection),
            defaultValue: RunningDirection.EndToStart);

    public RunningDirection RunningDirection
    {
        get => GetValue(RunningDirectionProperty);
        set => SetValue(RunningDirectionProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_elementPanel != null)
        {
            _elementPanel.SizeChanged -= ElementPanel_SizeChanged;
        }

        _elementContent = e.NameScope.Find<Control>(ElementContent);
        _elementPanel = e.NameScope.Find<Panel>(ElementPanel);

        if (_elementPanel != null)
        {
            _elementPanel.SizeChanged += ElementPanel_SizeChanged;
        }

        UpdateContent();
    }

    private void ElementPanel_SizeChanged(object? sender, SizeChangedEventArgs e) => UpdateContent();

    private void UpdateContent()
    {
        if (_elementContent == null || _elementPanel == null)
        {
            return;
        }

        if (MathHelper.IsZero(_elementPanel.Bounds.Width) || MathHelper.IsZero(_elementPanel.Bounds.Height))
        {
            return;
        }

        _animationCts?.Cancel();

        double from;
        double to;

        if (Orientation == Orientation.Horizontal)
        {
            if (AutoRun && _elementPanel.Bounds.Width < Bounds.Width)
            {
                return;
            }

            if (Runaway)
            {
                from = -_elementPanel.Bounds.Width;
                to = Bounds.Width;
            }
            else
            {
                from = 0;
                to = Bounds.Width - _elementPanel.Bounds.Width;
                SetCurrentValue(AutoReverseProperty, true);
            }
        }
        else
        {
            if (AutoRun && _elementPanel.Bounds.Height < Bounds.Height)
            {
                return;
            }

            if (Runaway)
            {
                from = -_elementPanel.Bounds.Height;
                to = Bounds.Height;
            }
            else
            {
                from = 0;
                to = Bounds.Height - _elementPanel.Bounds.Height;
                SetCurrentValue(AutoReverseProperty, true);
            }
        }

        var duration = double.IsNaN(Speed)
            ? Duration
            : !MathHelper.IsVerySmall(Speed)
                ? TimeSpan.FromSeconds(Math.Abs(to - from) / Speed)
                : Duration;

        var property = Orientation == Orientation.Horizontal
            ? TranslateTransform.XProperty
            : TranslateTransform.YProperty;

        var animation = new Animation
        {
            Duration = duration,
            Easing = new LinearEasing(),
            FillMode = FillMode.Forward,
            IterationCount = IterationCount.Infinite,
            PlaybackDirection = AutoReverse ? PlaybackDirection.Alternate : PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(property, RunningDirection == RunningDirection.StartToEnd ? from : to) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(property, RunningDirection == RunningDirection.StartToEnd ? to : from) }
                }
            }
        };

        _translateTransform ??= new TranslateTransform();
        _elementContent.RenderTransform = _translateTransform;

        if (!IsRunning)
        {
            return;
        }

        _animationCts = new CancellationTokenSource();
        _ = animation.RunAsync(_translateTransform, _animationCts.Token);
    }
}
