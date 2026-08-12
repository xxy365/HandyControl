using System.Collections.Generic;
using Avalonia.Media;

namespace HandyControl.Tools.Extension;

/// <summary>
///     颜色扩展类
/// </summary>
public static class ColorExtension
{
    internal static List<byte> ToList(this Color color) => new()
    {
        color.R,
        color.G,
        color.B
    };
}
