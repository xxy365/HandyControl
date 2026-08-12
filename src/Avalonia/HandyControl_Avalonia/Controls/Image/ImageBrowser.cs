using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace HandyControl.Controls;

public class ImageBrowser : Avalonia.Controls.Window
{
    private const string ElementPanelTop = "PART_PanelTop";

    private const string ElementImageViewer = "PART_ImageViewer";

    private const string ElementButtonClose = "PART_ButtonClose";

    private Panel? _panelTop;

    private ImageViewer? _imageViewer;

    public ImageBrowser()
    {
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.None;
        Topmost = true;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        Background = Brushes.Transparent;
        CanResize = true;
    }

    public ImageBrowser(Uri uri) : this()
    {
        _pendingUri = uri;
    }

    public ImageBrowser(string path) : this(new Uri(path))
    {
    }

    private Uri? _pendingUri;

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        LoadImage();
    }

    private void LoadImage()
    {
        if (_pendingUri == null || _imageViewer == null) return;

        try
        {
            var uri = _pendingUri;
            if (!uri.IsFile) return;

            _imageViewer.ImageSource = new Bitmap(uri.LocalPath);
            _imageViewer.ImgPath = uri.AbsolutePath;

            if (File.Exists(_imageViewer.ImgPath))
            {
                var info = new FileInfo(_imageViewer.ImgPath);
                _imageViewer.ImgSize = info.Length;
            }
        }
        catch
        {
            MessageBox.Show(Properties.Langs.Lang.ErrorImgPath);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_panelTop != null)
        {
            _panelTop.PointerPressed -= PanelTop_OnPointerPressed;
        }

        if (_imageViewer != null)
        {
            _imageViewer.PointerPressed -= ImageViewer_OnPointerPressed;
        }

        _panelTop = e.NameScope.Find<Panel>(ElementPanelTop);
        _imageViewer = e.NameScope.Find<ImageViewer>(ElementImageViewer);

        if (e.NameScope.Find<Button>(ElementButtonClose) is { } buttonClose)
        {
            buttonClose.Click += ButtonClose_OnClick;
        }

        if (_panelTop != null)
        {
            _panelTop.PointerPressed += PanelTop_OnPointerPressed;
        }

        if (_imageViewer != null)
        {
            _imageViewer.PointerPressed += ImageViewer_OnPointerPressed;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _imageViewer?.Dispose();
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e) => Close();

    private void PanelTop_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void ImageViewer_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
            _imageViewer != null &&
            !(_imageViewer.ImageWidth > Bounds.Width || _imageViewer.ImageHeight > Bounds.Height))
        {
            BeginMoveDrag(e);
        }
    }
}