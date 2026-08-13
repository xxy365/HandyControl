using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using HandyControl.Data;

namespace HandyControl.Controls;

public sealed class Notification : Avalonia.Controls.Window
{
    private const int WaitTime = 6;

    private const int TotalFrames = 30;

    private const int FrameIntervalMs = 16;

    private int _tickCount;

    private DispatcherTimer? _timerClose;

    private readonly CancellationTokenSource _animationCts = new();

    private ShowAnimation ShowAnimation { get; set; }

    private bool _shouldBeClosed;

    public Notification()
    {
        WindowDecorations = WindowDecorations.None;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Blur };
        Topmost = true;
    }

    public static Notification Show(object content, ShowAnimation showAnimation = ShowAnimation.None, bool staysOpen = false)
    {
        var notification = new Notification
        {
            Content = content,
            Opacity = 0,
            ShowAnimation = showAnimation
        };

        if (content is Control control)
        {
            control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var desired = control.DesiredSize;
            notification.Width = desired.Width > 0 ? desired.Width : 320;
            notification.Height = desired.Height > 0 ? desired.Height : 200;
        }

        notification.Show();

        var workingArea = notification.GetWorkingArea();

        var leftMax = workingArea.Width - (int)notification.Width;
        var topMax = workingArea.Height - (int)notification.Height;
        var target = new Point(leftMax, topMax);

        switch (showAnimation)
        {
            case ShowAnimation.None:
                notification.Opacity = 1;
                notification.Position = new PixelPoint((int)target.X, (int)target.Y);
                break;
            case ShowAnimation.HorizontalMove:
                notification.RunPositionAnimation(new Point(workingArea.Width, topMax), target);
                notification.Opacity = 1;
                break;
            case ShowAnimation.VerticalMove:
                notification.RunPositionAnimation(new Point(leftMax, workingArea.Height), target);
                notification.Opacity = 1;
                break;
            case ShowAnimation.Fade:
                notification.Position = new PixelPoint((int)target.X, (int)target.Y);
                notification.RunOpacityAnimation(1);
                break;
            default:
                notification.Opacity = 1;
                notification.Position = new PixelPoint((int)target.X, (int)target.Y);
                break;
        }

        if (!staysOpen)
        {
            notification.StartTimer();
        }

        return notification;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (_shouldBeClosed)
        {
            return;
        }

        _shouldBeClosed = true;
        e.Cancel = true;

        var workingArea = GetWorkingArea();

        switch (ShowAnimation)
        {
            case ShowAnimation.None:
            case ShowAnimation.HorizontalMove:
            case ShowAnimation.VerticalMove:
                var target = new Point(workingArea.Width, workingArea.Height);
                if (ShowAnimation == ShowAnimation.HorizontalMove)
                {
                    RunPositionAnimation(new Point(Position.X, Position.Y), new Point(workingArea.Width, Position.Y), Close);
                }
                else if (ShowAnimation == ShowAnimation.VerticalMove)
                {
                    RunPositionAnimation(new Point(Position.X, Position.Y), new Point(Position.X, workingArea.Height), Close);
                }
                else
                {
                    Close();
                }
                break;
            case ShowAnimation.Fade:
                RunOpacityAnimation(0, Close);
                break;
            default:
                Close();
                break;
        }
    }

    private PixelRect GetWorkingArea()
    {
        return Screens.ScreenFromWindow(this)?.WorkingArea
               ?? Screens.Primary?.WorkingArea
               ?? new PixelRect(new PixelPoint(0, 0), new PixelSize(1920, 1080));
    }

    private void StartTimer()
    {
        _timerClose = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timerClose.Tick += delegate
        {
            if (IsPointerOver)
            {
                _tickCount = 0;
                return;
            }

            _tickCount++;
            if (_tickCount >= WaitTime)
            {
                Close();
            }
        };
        _timerClose.Start();
    }

    private void RunPositionAnimation(Point from, Point to, Action? onComplete = null)
    {
        _ = AnimatePositionAsync(from, to, onComplete);
    }

    private async Task AnimatePositionAsync(Point from, Point to, Action? onComplete)
    {
        for (var frame = 1; frame <= TotalFrames; frame++)
        {
            if (_animationCts.IsCancellationRequested)
            {
                return;
            }

            await Task.Delay(FrameIntervalMs);
            var progress = frame / (double)TotalFrames;
            Position = new PixelPoint(
                (int)(from.X + (to.X - from.X) * progress),
                (int)(from.Y + (to.Y - from.Y) * progress));
        }

        onComplete?.Invoke();
    }

    private void RunOpacityAnimation(double to, Action? onComplete = null)
    {
        var from = Opacity;
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(TotalFrames * FrameIntervalMs),
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(Visual.OpacityProperty, from) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(Visual.OpacityProperty, to) }
                }
            }
        };

        _ = RunAnimation(animation, onComplete);
    }

    private async Task RunAnimation(Animation animation, Action? onComplete)
    {
        try
        {
            await animation.RunAsync(this, _animationCts.Token);
        }
        catch (OperationCanceledException)
        {
        }

        onComplete?.Invoke();
    }
}