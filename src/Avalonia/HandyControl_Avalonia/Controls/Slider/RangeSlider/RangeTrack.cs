using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace HandyControl.Controls;

public class RangeTrack : Control
{
    private RepeatButton? _increaseButton;

    private RepeatButton? _centerButton;

    private RepeatButton? _decreaseButton;

    private RangeThumb? _thumbStart;

    private RangeThumb? _thumbEnd;

    private double Density { get; set; } = double.NaN;

    public RepeatButton? DecreaseRepeatButton
    {
        get => _decreaseButton;
        set => UpdateComponent(ref _decreaseButton, value);
    }

    public RepeatButton? CenterRepeatButton
    {
        get => _centerButton;
        set => UpdateComponent(ref _centerButton, value);
    }

    public RepeatButton? IncreaseRepeatButton
    {
        get => _increaseButton;
        set => UpdateComponent(ref _increaseButton, value);
    }

    public RangeThumb? ThumbStart
    {
        get => _thumbStart;
        set => UpdateComponent(ref _thumbStart, value);
    }

    public RangeThumb? ThumbEnd
    {
        get => _thumbEnd;
        set => UpdateComponent(ref _thumbEnd, value);
    }

    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<RangeTrack, Orientation>(
        nameof(Orientation));

    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<RangeTrack, double>(
        nameof(Minimum), 0.0);

    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<RangeTrack, double>(
        nameof(Maximum), 1.0);

    public static readonly StyledProperty<double> ValueStartProperty = AvaloniaProperty.Register<RangeTrack, double>(
        nameof(ValueStart), 0.0);

    public static readonly StyledProperty<double> ValueEndProperty = AvaloniaProperty.Register<RangeTrack, double>(
        nameof(ValueEnd), 0.0);

    public static readonly StyledProperty<bool> IsDirectionReversedProperty = AvaloniaProperty.Register<RangeTrack, bool>(
        nameof(IsDirectionReversed));

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double ValueStart
    {
        get => GetValue(ValueStartProperty);
        set => SetValue(ValueStartProperty, value);
    }

    public double ValueEnd
    {
        get => GetValue(ValueEndProperty);
        set => SetValue(ValueEndProperty, value);
    }

    public bool IsDirectionReversed
    {
        get => GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    static RangeTrack()
    {
        AffectsArrange<RangeTrack>(MinimumProperty, MaximumProperty, ValueStartProperty, ValueEndProperty,
            IsDirectionReversedProperty, OrientationProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var desiredSize = new Size();

        if (_thumbStart != null)
        {
            _thumbStart.Measure(availableSize);
            desiredSize = _thumbStart.DesiredSize;
        }

        if (_thumbEnd != null)
        {
            _thumbEnd.Measure(availableSize);
            desiredSize = new Size(Math.Max(_thumbEnd.DesiredSize.Width, desiredSize.Width),
                Math.Max(_thumbEnd.DesiredSize.Height, desiredSize.Height));
        }

        return desiredSize;
    }

    private static void CoerceLength(ref double componentLength, double trackLength)
    {
        if (componentLength < 0)
        {
            componentLength = 0;
        }
        else if (componentLength > trackLength || double.IsNaN(componentLength))
        {
            componentLength = trackLength;
        }
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var isVertical = Orientation == Orientation.Vertical;

        ComputeLengths(arrangeSize, isVertical, out var decreaseButtonLength, out var centerButtonLength,
            out var increaseButtonLength, out var thumbStartLength, out var thumbEndLength);

        var offset = new Point();
        var pieceSize = arrangeSize;
        var isDirectionReversed = IsDirectionReversed;

        if (isVertical)
        {
            CoerceLength(ref decreaseButtonLength, arrangeSize.Height);
            CoerceLength(ref centerButtonLength, arrangeSize.Height);
            CoerceLength(ref increaseButtonLength, arrangeSize.Height);
            CoerceLength(ref thumbStartLength, arrangeSize.Height);
            CoerceLength(ref thumbEndLength, arrangeSize.Height);

            offset = offset.WithY(isDirectionReversed ? decreaseButtonLength + thumbEndLength + centerButtonLength + thumbStartLength : 0);
            pieceSize = pieceSize.WithHeight(increaseButtonLength);

            IncreaseRepeatButton?.Arrange(new Rect(offset, pieceSize));

            offset = offset.WithY(isDirectionReversed ? decreaseButtonLength + thumbEndLength : increaseButtonLength + thumbStartLength);
            pieceSize = pieceSize.WithHeight(centerButtonLength);

            CenterRepeatButton?.Arrange(new Rect(offset, pieceSize));

            offset = offset.WithY(isDirectionReversed ? 0 : increaseButtonLength + thumbStartLength + centerButtonLength + thumbEndLength);
            pieceSize = pieceSize.WithHeight(decreaseButtonLength);

            DecreaseRepeatButton?.Arrange(new Rect(offset, pieceSize));

            offset = offset.WithY(isDirectionReversed
                ? decreaseButtonLength + thumbEndLength + centerButtonLength
                : increaseButtonLength + thumbStartLength + centerButtonLength);
            pieceSize = pieceSize.WithHeight(thumbStartLength);

            ArrangeThumb(isDirectionReversed, false, offset, pieceSize);

            offset = offset.WithY(isDirectionReversed ? decreaseButtonLength : increaseButtonLength);
            pieceSize = pieceSize.WithHeight(thumbEndLength);

            ArrangeThumb(isDirectionReversed, true, offset, pieceSize);
        }
        else
        {
            CoerceLength(ref decreaseButtonLength, arrangeSize.Width);
            CoerceLength(ref centerButtonLength, arrangeSize.Width);
            CoerceLength(ref increaseButtonLength, arrangeSize.Width);
            CoerceLength(ref thumbStartLength, arrangeSize.Width);
            CoerceLength(ref thumbEndLength, arrangeSize.Width);

            offset = offset.WithX(isDirectionReversed ? 0 : decreaseButtonLength + thumbEndLength + centerButtonLength + thumbStartLength);
            pieceSize = pieceSize.WithWidth(increaseButtonLength);

            IncreaseRepeatButton?.Arrange(new Rect(offset, pieceSize));

            offset = offset.WithX(isDirectionReversed ? increaseButtonLength + thumbStartLength : decreaseButtonLength + thumbEndLength);
            pieceSize = pieceSize.WithWidth(centerButtonLength);

            CenterRepeatButton?.Arrange(new Rect(offset, pieceSize));

            offset = offset.WithX(isDirectionReversed ? increaseButtonLength + thumbStartLength + centerButtonLength + thumbEndLength : 0);
            pieceSize = pieceSize.WithWidth(decreaseButtonLength);

            DecreaseRepeatButton?.Arrange(new Rect(offset, pieceSize));

            offset = offset.WithX(isDirectionReversed ? increaseButtonLength : decreaseButtonLength);
            pieceSize = pieceSize.WithWidth(thumbStartLength);

            ArrangeThumb(isDirectionReversed, false, offset, pieceSize);

            offset = offset.WithX(isDirectionReversed
                ? increaseButtonLength + thumbStartLength + centerButtonLength
                : decreaseButtonLength + thumbEndLength + centerButtonLength);
            pieceSize = pieceSize.WithWidth(thumbEndLength);

            ArrangeThumb(isDirectionReversed, true, offset, pieceSize);
        }

        return arrangeSize;
    }

    private void ArrangeThumb(bool isDirectionReversed, bool isStart, Point offset, Size pieceSize)
    {
        if (isStart)
        {
            if (isDirectionReversed)
            {
                ThumbStart?.Arrange(new Rect(offset, pieceSize));
            }
            else
            {
                ThumbEnd?.Arrange(new Rect(offset, pieceSize));
            }
        }
        else
        {
            if (isDirectionReversed)
            {
                ThumbEnd?.Arrange(new Rect(offset, pieceSize));
            }
            else
            {
                ThumbStart?.Arrange(new Rect(offset, pieceSize));
            }
        }
    }

    private void ComputeLengths(Size arrangeSize, bool isVertical, out double decreaseButtonLength,
        out double centerButtonLength, out double increaseButtonLength, out double thumbStartLength,
        out double thumbEndLength)
    {
        var min = Minimum;
        var range = Math.Max(0.0, Maximum - min);
        var offsetStart = Math.Min(range, ValueStart - min);
        var offsetEnd = Math.Min(range, ValueEnd - min);

        double trackLength;

        if (isVertical)
        {
            trackLength = arrangeSize.Height;
            thumbStartLength = _thumbStart?.DesiredSize.Height ?? 0;
            thumbEndLength = _thumbEnd?.DesiredSize.Height ?? 0;
        }
        else
        {
            trackLength = arrangeSize.Width;
            thumbStartLength = _thumbStart?.DesiredSize.Width ?? 0;
            thumbEndLength = _thumbEnd?.DesiredSize.Width ?? 0;
        }

        CoerceLength(ref thumbStartLength, trackLength);
        CoerceLength(ref thumbEndLength, trackLength);

        var remainingTrackLength = trackLength - thumbStartLength - thumbEndLength;

        decreaseButtonLength = remainingTrackLength * offsetStart / range;
        CoerceLength(ref decreaseButtonLength, remainingTrackLength);

        centerButtonLength = remainingTrackLength * offsetEnd / range - decreaseButtonLength;
        CoerceLength(ref centerButtonLength, remainingTrackLength);

        increaseButtonLength = remainingTrackLength - decreaseButtonLength - centerButtonLength;
        CoerceLength(ref increaseButtonLength, remainingTrackLength);

        Density = range / remainingTrackLength;
    }

    public virtual double ValueFromPoint(Point pt)
    {
        return Orientation == Orientation.Horizontal
            ? !IsDirectionReversed
                ? pt.X / Bounds.Width * Maximum
                : (1 - pt.X / Bounds.Width) * Maximum
            : !IsDirectionReversed
                ? pt.Y / Bounds.Height * Maximum
                : (1 - pt.Y / Bounds.Height) * Maximum;
    }

    public virtual double ValueFromDistance(double horizontal, double vertical)
    {
        double scale = IsDirectionReversed ? -1 : 1;
        return Orientation == Orientation.Horizontal
            ? scale * horizontal * Density
            : -1 * scale * vertical * Density;
    }

    private void UpdateComponent<T>(ref T? field, T? newValue) where T : Control
    {
        if (field == newValue) return;

        if (field != null)
        {
            VisualChildren.Remove(field);
            LogicalChildren.Remove(field);
        }

        field = newValue;

        if (field != null)
        {
            VisualChildren.Add(field);
            LogicalChildren.Add(field);
        }

        InvalidateMeasure();
        InvalidateArrange();
    }
}