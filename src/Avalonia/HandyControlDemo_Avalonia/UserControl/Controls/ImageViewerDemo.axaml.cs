using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using HandyControl.Controls;

namespace HandyControlDemo.UserControl;

public partial class ImageViewerDemo : Avalonia.Controls.UserControl
{
    private readonly string _imagePath;

    public ImageViewerDemo()
    {
        InitializeComponent();
        _imagePath = ExtractDemoImage("avares://HandyControlDemo/Resources/Img/b1.jpg");
        MainImageViewer.Uri = new Uri(_imagePath);
    }

    private static string ExtractDemoImage(string resourceUri)
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri(resourceUri));
            var dest = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "hc_b1.jpg");
            using (var file = System.IO.File.Create(dest))
            {
                stream.CopyTo(file);
            }
            return dest;
        }
        catch
        {
            return resourceUri;
        }
    }

    private void ShowImageBrowser_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var browser = new ImageBrowser(new Uri(_imagePath))
        {
            Width = 900,
            Height = 600
        };

        if (TopLevel.GetTopLevel(this) is Avalonia.Controls.Window owner)
        {
            browser.ShowDialog(owner);
        }
        else
        {
            browser.Show();
        }
    }
}