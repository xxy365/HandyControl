using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using HandyControl.Tools;

namespace HandyControlDemo.Tools.Converter;

public class StringToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string str ? ResourceHelper.GetResource<IBrush>(str) : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}