using Avalonia;
using Avalonia.Controls;

namespace HandyControl.Controls;

public sealed class Sprite : Avalonia.Controls.Window
{
    private Sprite()
    {
        WindowDecorations = WindowDecorations.None;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Blur };
    }

    public static Sprite Show(object content)
    {
        var sprite = new Sprite
        {
            Content = content
        };

        sprite.Show();

        var workingArea = sprite.Screens.ScreenFromWindow(sprite)?.WorkingArea
                          ?? sprite.Screens.Primary?.WorkingArea
                          ?? new PixelRect(new PixelPoint(0, 0), new PixelSize(1920, 1080));

        sprite.Position = new PixelPoint(
            workingArea.Width - (int)sprite.Bounds.Width - 50,
            50 - (int)sprite.Padding.Top);

        return sprite;
    }
}