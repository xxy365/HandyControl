using System.Globalization;
using Avalonia.Media;

namespace HandyControl.Tools.Helper;

internal class TextHelper
{
    public static FormattedText CreateFormattedText(string text, FlowDirection flowDirection, Typeface typeface, double fontSize)
    {
        return new FormattedText(
            text,
            CultureInfo.CurrentUICulture,
            flowDirection,
            typeface,
            fontSize,
            Brushes.Black);
    }
}