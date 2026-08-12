using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace HandyControl.Controls;

public class CompareTrack : Track
{
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        base.ArrangeOverride(arrangeSize);

        var isVertical = Orientation == Orientation.Vertical;
        ComputeSliderLengths(arrangeSize, isVertical, out var decreaseButtonLength, out var thumbLength,
            out var increaseButtonLength);

        var offset = new Point();
        var pieceSize = arrangeSize;
        var isDirectionReversed = IsDirectionReversed;

        if (isVertical)
        {
            CoerceLength(ref decreaseButtonLength, arrangeSize.Height);
            CoerceLength(ref increaseButtonLength, arrangeSize.Height);
            CoerceLength(ref thumbLength, arrangeSize.Height);

            offset = offset.WithY(isDirectionReversed ? decreaseButtonLength + thumbLength : 0.0);
            pieceSize = pieceSize.WithHeight(increaseButtonLength);

            IncreaseButton?.Arrange(new Rect(offset, pieceSize));

            offset = offset.WithY(isDirectionReversed ? 0.0 : increaseButtonLength + thumbLength);
            pieceSize = pieceSize.WithHeight(decreaseButtonLength);

            if (DecreaseButton != null)
            {
                DecreaseButton.Arrange(new Rect(offset, pieceSize));
                DecreaseButton.Height = pieceSize.Height;
            }

            offset = offset.WithY(isDirectionReversed ? decreaseButtonLength : increaseButtonLength);
            pieceSize = pieceSize.WithHeight(thumbLength);

            Thumb?.Arrange(new Rect(offset, pieceSize));
        }
        else
        {
            CoerceLength(ref decreaseButtonLength, arrangeSize.Width);
            CoerceLength(ref increaseButtonLength, arrangeSize.Width);
            CoerceLength(ref thumbLength, arrangeSize.Width);

            offset = offset.WithX(isDirectionReversed ? increaseButtonLength + thumbLength : 0.0);
            pieceSize = pieceSize.WithWidth(decreaseButtonLength);

            DecreaseButton?.Arrange(new Rect(offset, pieceSize));

            offset = offset.WithX(isDirectionReversed ? 0.0 : decreaseButtonLength + thumbLength);
            pieceSize = pieceSize.WithWidth(increaseButtonLength);

            if (IncreaseButton != null)
            {
                IncreaseButton.Arrange(new Rect(offset, pieceSize));
                IncreaseButton.Width = pieceSize.Width;
            }

            offset = offset.WithX(isDirectionReversed ? increaseButtonLength : decreaseButtonLength);
            pieceSize = pieceSize.WithWidth(thumbLength);

            Thumb?.Arrange(new Rect(offset, pieceSize));
        }

        return arrangeSize;
    }

    private void ComputeSliderLengths(Size arrangeSize, bool isVertical, out double decreaseButtonLength,
        out double thumbLength, out double increaseButtonLength)
    {
        var min = Minimum;
        var range = Math.Max(0.0, Maximum - min);
        var offset = Math.Min(range, Value - min);

        double trackLength;

        if (isVertical)
        {
            trackLength = arrangeSize.Height;
            thumbLength = Thumb?.DesiredSize.Height ?? 0;
        }
        else
        {
            trackLength = arrangeSize.Width;
            thumbLength = Thumb?.DesiredSize.Width ?? 0;
        }

        CoerceLength(ref thumbLength, trackLength);

        var remainingTrackLength = trackLength - thumbLength;

        decreaseButtonLength = remainingTrackLength * offset / range;
        CoerceLength(ref decreaseButtonLength, remainingTrackLength);

        increaseButtonLength = remainingTrackLength - decreaseButtonLength;
        CoerceLength(ref increaseButtonLength, remainingTrackLength);
    }

    private static void CoerceLength(ref double componentLength, double trackLength)
    {
        if (componentLength < 0)
            componentLength = 0.0;
        else if (componentLength > trackLength || double.IsNaN(componentLength))
            componentLength = trackLength;
    }
}