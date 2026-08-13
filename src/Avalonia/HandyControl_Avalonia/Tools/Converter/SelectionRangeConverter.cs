using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace HandyControl.Tools.Converter;

/// <summary>
///     计算 Slider 选中区段在轨道上的偏移与尺寸。
///     输入顺序：[Minimum, Maximum, SelectionStart, SelectionEnd, 轨道像素长度]。
///     ConverterParameter："Offset" 返回 Thickness（横向为 Left、纵向为 Top 的偏移），"Length" 返回区段长度。
/// </summary>
public class SelectionRangeConverter : IMultiValueConverter
{
    public object? Convert(IList<object?>? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not { Count: >= 5 })
        {
            return parameter is "Length" ? 0d : new Thickness();
        }

        if (values[0] is not double minimum
            || values[1] is not double maximum
            || values[2] is not double selectionStart
            || values[3] is not double selectionEnd
            || values[4] is not double trackLength)
        {
            return parameter is "Length" ? 0d : new Thickness();
        }

        var range = maximum - minimum;
        double offset, length;

        if (Math.Abs(range) < 0.0001)
        {
            offset = 0d;
            length = trackLength;
        }
        else
        {
            offset = Math.Clamp((selectionStart - minimum) / range, 0d, 1d) * trackLength;
            length = Math.Clamp((selectionEnd - selectionStart) / range, 0d, 1d) * trackLength;
        }

        if (parameter is "Length")
        {
            return length;
        }

        if (parameter is "Vertical")
        {
            return new Thickness(0d, offset, 0d, 0d);
        }

        return new Thickness(offset, 0d, 0d, 0d);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}