using System;
using System.IO;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace HandyControl.Controls;

public class ImageViewer : TemplatedControl, IDisposable
{
    private const string ElementPanelMain = "PART_PanelMain";

    private const string ElementCanvasSmallImg = "PART_CanvasSmallImg";

    private const string ElementBorderMove = "PART_BorderMove";

    private const string ElementBorderBottom = "PART_BorderBottom";

    private const string ElementImageMain = "PART_ImageMain";

    private const string ElementButtonSave = "PART_ButtonSave";

    private const string ElementButtonOpen = "PART_ButtonOpen";

    private const string ElementButtonReduce = "PART_ButtonReduce";

    private const string ElementButtonEnlarge = "PART_ButtonEnlarge";

    private const string ElementButtonRestore = "PART_ButtonRestore";

    private const string ElementButtonRotateLeft = "PART_ButtonRotateLeft";

    private const string ElementButtonRotateRight = "PART_ButtonRotateRight";

    private const double ScaleInternal = 0.2;

    private Panel? _panelMain;

    private Canvas? _canvasSmallImg;

    private Border? _borderMove;

    private Border? _borderBottom;

    private Image? _imageMain;

    private bool _borderSmallIsLoaded;

    private bool _canMoveX;

    private bool _canMoveY;

    private Thickness _imgActualMargin;

    private double _imgActualRotate;

    private double _imgActualScale = 1;

    private Point _imgCurrentPoint;

    private bool _imgIsMouseDown;

    private Thickness _imgMouseDownMargin;

    private Point _imgMouseDownPoint;

    private Point _imgSmallCurrentPoint;

    private bool _imgSmallIsMouseDown;

    private Thickness _imgSmallMouseDownMargin;

    private Point _imgSmallMouseDownPoint;

    private Point _lastPosInCanvas;

    private double _imgWidHeiScale;

    private bool _isOblique;

    private double _scaleInternalHeight;

    private double _scaleInternalWidth;

    private bool _showBorderBottom;

    private bool _isLoaded;

    private bool _isInitialized;

    private bool _isDisposed;

    private double _imageOriPixelWidth;

    private double _imageOriPixelHeight;

    private ImageBrowser? _imageBrowser;

    public ImageViewer()
    {
        ImageRotateProperty.Changed.AddClassHandler<ImageViewer>((o, _) => o.OnImageRotateChanged());
    }

    public ImageViewer(Uri uri) : this()
    {
        Uri = uri;
    }

    public ImageViewer(string path) : this(new Uri(path))
    {
    }

    public static readonly StyledProperty<bool> ShowImgMapProperty =
        AvaloniaProperty.Register<ImageViewer, bool>(nameof(ShowImgMap));

    public static readonly StyledProperty<IImage?> ImageSourceProperty =
        AvaloniaProperty.Register<ImageViewer, IImage?>(nameof(ImageSource));

    public static readonly StyledProperty<Uri?> UriProperty =
        AvaloniaProperty.Register<ImageViewer, Uri?>(nameof(Uri));

    public static readonly StyledProperty<bool> ShowToolBarProperty =
        AvaloniaProperty.Register<ImageViewer, bool>(nameof(ShowToolBar), true);

    public static readonly StyledProperty<bool> IsFullScreenProperty =
        AvaloniaProperty.Register<ImageViewer, bool>(nameof(IsFullScreen), false);

    public static readonly StyledProperty<bool> ShowFullScreenButtonProperty =
        AvaloniaProperty.Register<ImageViewer, bool>(nameof(ShowFullScreenButton));

    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<ImageViewer, bool>(nameof(ShowCloseButton));

    public static readonly StyledProperty<object?> ImageContentProperty =
        AvaloniaProperty.Register<ImageViewer, object?>(nameof(ImageContent));

    public static readonly StyledProperty<Thickness> ImageMarginProperty =
        AvaloniaProperty.Register<ImageViewer, Thickness>(nameof(ImageMargin));

    public static readonly StyledProperty<double> ImageWidthProperty =
        AvaloniaProperty.Register<ImageViewer, double>(nameof(ImageWidth));

    public static readonly StyledProperty<double> ImageHeightProperty =
        AvaloniaProperty.Register<ImageViewer, double>(nameof(ImageHeight));

    public static readonly StyledProperty<double> ImageScaleProperty =
        AvaloniaProperty.Register<ImageViewer, double>(nameof(ImageScale), 1);

    public static readonly StyledProperty<string> ScaleStrProperty =
        AvaloniaProperty.Register<ImageViewer, string>(nameof(ScaleStr), "100%");

    public static readonly StyledProperty<double> ImageRotateProperty =
        AvaloniaProperty.Register<ImageViewer, double>(nameof(ImageRotate));

    public static readonly StyledProperty<bool> ShowSmallImgInternalProperty =
        AvaloniaProperty.Register<ImageViewer, bool>(nameof(ShowSmallImgInternal));

    public static readonly StyledProperty<string> ImgPathProperty =
        AvaloniaProperty.Register<ImageViewer, string>(nameof(ImgPath));

    public static readonly StyledProperty<long> ImgSizeProperty =
        AvaloniaProperty.Register<ImageViewer, long>(nameof(ImgSize), -1);

    public bool IsFullScreen
    {
        get => GetValue(IsFullScreenProperty);
        set => SetValue(IsFullScreenProperty, value);
    }

    public bool ShowImgMap
    {
        get => GetValue(ShowImgMapProperty);
        set => SetValue(ShowImgMapProperty, value);
    }

    public IImage? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public Uri? Uri
    {
        get => GetValue(UriProperty);
        set => SetValue(UriProperty, value);
    }

    public bool ShowToolBar
    {
        get => GetValue(ShowToolBarProperty);
        set => SetValue(ShowToolBarProperty, value);
    }

    public object? ImageContent
    {
        get => GetValue(ImageContentProperty);
        set => SetValue(ImageContentProperty, value);
    }

    public string ImgPath
    {
        get => GetValue(ImgPathProperty);
        set => SetValue(ImgPathProperty, value);
    }

    public long ImgSize
    {
        get => GetValue(ImgSizeProperty);
        set => SetValue(ImgSizeProperty, value);
    }

    public bool ShowFullScreenButton
    {
        get => GetValue(ShowFullScreenButtonProperty);
        internal set => SetValue(ShowFullScreenButtonProperty, value);
    }

    public Thickness ImageMargin
    {
        get => GetValue(ImageMarginProperty);
        internal set => SetValue(ImageMarginProperty, value);
    }

    public double ImageWidth
    {
        get => GetValue(ImageWidthProperty);
        internal set => SetValue(ImageWidthProperty, value);
    }

    public double ImageHeight
    {
        get => GetValue(ImageHeightProperty);
        internal set => SetValue(ImageHeightProperty, value);
    }

    internal double ImageScale
    {
        get => GetValue(ImageScaleProperty);
        set => SetValue(ImageScaleProperty, value);
    }

    public string ScaleStr
    {
        get => GetValue(ScaleStrProperty);
        internal set => SetValue(ScaleStrProperty, value);
    }

    internal double ImageRotate
    {
        get => GetValue(ImageRotateProperty);
        set => SetValue(ImageRotateProperty, value);
    }

    public bool ShowSmallImgInternal
    {
        get => GetValue(ShowSmallImgInternalProperty);
        internal set => SetValue(ShowSmallImgInternalProperty, value);
    }

    private double ImageOriWidth { get; set; }

    private double ImageOriHeight { get; set; }

    public bool ShowCloseButton
    {
        get => GetValue(ShowCloseButtonProperty);
        internal set => SetValue(ShowCloseButtonProperty, value);
    }

    internal bool ShowBorderBottom
    {
        get => _showBorderBottom;
        set
        {
            if (_showBorderBottom == value) return;
            if (_borderBottom != null)
            {
                BeginAnimation(_borderBottom, OpacityProperty, value ? 1d : 0d, value ? 100 : 400);
            }
            _showBorderBottom = value;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _panelMain = e.NameScope.Find<Panel>(ElementPanelMain);
        _canvasSmallImg = e.NameScope.Find<Canvas>(ElementCanvasSmallImg);
        _borderMove = e.NameScope.Find<Border>(ElementBorderMove);
        _imageMain = e.NameScope.Find<Image>(ElementImageMain);
        _borderBottom = e.NameScope.Find<Border>(ElementBorderBottom);

        if (_imageMain != null)
        {
            _imageMain.RenderTransform = new RotateTransform();
            _imageMain.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        }

        if (_canvasSmallImg != null)
        {
            _canvasSmallImg.PointerPressed += CanvasSmallImg_OnPointerPressed;
            _canvasSmallImg.PointerReleased += CanvasSmallImg_OnPointerReleased;
            _canvasSmallImg.PointerMoved += CanvasSmallImg_OnPointerMoved;
        }

        WireButton(e, ElementButtonSave, ButtonSave_OnClick);
        WireButton(e, ElementButtonOpen, ButtonWindowsOpen_OnClick);
        WireButton(e, ElementButtonReduce, ButtonReduce_OnClick);
        WireButton(e, ElementButtonEnlarge, ButtonEnlarge_OnClick);
        WireButton(e, ElementButtonRestore, ButtonActual_OnClick);
        WireButton(e, ElementButtonRotateLeft, ButtonRotateLeft_OnClick);
        WireButton(e, ElementButtonRotateRight, ButtonRotateRight_OnClick);

        _borderSmallIsLoaded = false;
    }

    private static void WireButton(TemplateAppliedEventArgs e, string name, EventHandler<Avalonia.Interactivity.RoutedEventArgs> handler)
    {
        if (e.NameScope.Find<Button>(name) is { } button)
        {
            button.Click += handler;
        }
    }

    private void OnImageRotateChanged()
    {
        if (_imageMain?.RenderTransform is RotateTransform rotateTransform)
        {
            rotateTransform.Angle = ImageRotate;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ImageScaleProperty && change.NewValue is double newValue)
        {
            ImageWidth = ImageOriWidth * newValue;
            ImageHeight = ImageOriHeight * newValue;
            ScaleStr = $"{newValue * 100:#0}%";
        }
        else if (change.Property == UriProperty)
        {
            OnUriChanged((Uri?)change.NewValue);
        }
        else if (change.Property == ImageSourceProperty)
        {
            Init();
        }
    }

    private void Init()
    {
        if (ImageSource == null || !_isLoaded) return;
        if (Bounds.Width < 1 || Bounds.Height < 1) return;

        var (width, height) = GetImagePixelSize(ImageSource);

        if (!_isOblique)
        {
            _imageOriPixelWidth = width;
            _imageOriPixelHeight = height;
        }

        var showWidth = _isOblique ? _imageOriPixelHeight : _imageOriPixelWidth;
        var showHeight = _isOblique ? _imageOriPixelWidth : _imageOriPixelHeight;

        ImageWidth = showWidth;
        ImageHeight = showHeight;
        ImageOriWidth = showWidth;
        ImageOriHeight = showHeight;
        _scaleInternalWidth = ImageOriWidth * ScaleInternal;
        _scaleInternalHeight = ImageOriHeight * ScaleInternal;

        if (Math.Abs(height - 0) < 0.001 || Math.Abs(width - 0) < 0.001)
        {
            MessageBox.Show(Properties.Langs.Lang.ErrorImgSize);
            return;
        }

        _imgWidHeiScale = width / height;
        var scaleWindow = Bounds.Width / Bounds.Height;
        ImageScale = 1;

        if (_imgWidHeiScale > scaleWindow)
        {
            if (width > Bounds.Width)
            {
                ImageScale = Bounds.Width / width;
            }
        }
        else if (height > Bounds.Height)
        {
            ImageScale = Bounds.Height / height;
        }

        ImageMargin = new Thickness((Bounds.Width - ImageWidth) / 2, (Bounds.Height - ImageHeight) / 2, 0, 0);

        _imgActualScale = ImageScale;
        _imgActualMargin = ImageMargin;

        _isInitialized = true;

        InitBorderSmall();
    }

    private static (double Width, double Height) GetImagePixelSize(IImage imageSource)
    {
        if (imageSource is Bitmap bitmap)
        {
            return (bitmap.PixelSize.Width, bitmap.PixelSize.Height);
        }

        return (imageSource.Size.Width, imageSource.Size.Height);
    }

    private void ButtonActual_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        BeginAnimation(this, ImageScaleProperty, 1d, 200);
        _imgActualScale = 1;
        var thickness = new Thickness((Bounds.Width - ImageOriWidth) / 2, (Bounds.Height - ImageOriHeight) / 2, 0, 0);
        BeginAnimation(this, ImageMarginProperty, thickness, 200);
        _imgActualMargin = thickness;
        _canMoveX = ImageWidth > Bounds.Width;
        _canMoveY = ImageHeight > Bounds.Height;
        BorderSmallShowSwitch();
    }

    private void ButtonReduce_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ScaleImg(false);

    private void ButtonEnlarge_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ScaleImg(true);

    private void ButtonRotateLeft_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => RotateImg(_imgActualRotate - 90);

    private void ButtonRotateRight_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => RotateImg(_imgActualRotate + 90);

    private async void ButtonSave_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ImageSource is not Bitmap bitmap) return;

        if (TopLevel.GetTopLevel(this) is not TopLevel topLevel)
        {
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
        {
            SuggestedFileName = $"{DateTime.Now:yyyy-M-d-h-m-s.fff}",
            DefaultExtension = "png",
            FileTypeChoices = new[]
            {
                new Avalonia.Platform.Storage.FilePickerFileType("Png") { Patterns = new[] { "*.png" } }
            }
        });

        if (file is not null)
        {
            bitmap.Save(file.Path.LocalPath, Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
        }
    }

    private void ButtonWindowsOpen_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Uri is { } uri)
        {
            _imageBrowser?.Close();
            _imageBrowser = new ImageBrowser(uri);
            _imageBrowser.Show();
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        MoveImg(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        ShowBorderBottom = false;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        ScaleImg(e.Delta.Y > 0);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (_isLoaded && ImageSource != null && !_isInitialized &&
            Bounds.Width > 0 && Bounds.Height > 0)
        {
            Init();
        }

        OnRenderSizeChanged();
    }

    private void OnRenderSizeChanged()
    {
        if (ImageWidth < 0.001 || ImageHeight < 0.001) return;

        _canMoveX = true;
        _canMoveY = true;

        var marginX = ImageMargin.Left;
        var marginY = ImageMargin.Top;

        if (ImageWidth <= Bounds.Width)
        {
            _canMoveX = false;
            marginX = (Bounds.Width - ImageWidth) / 2;
        }

        if (ImageHeight <= Bounds.Height)
        {
            _canMoveY = false;
            marginY = (Bounds.Height - ImageHeight) / 2;
        }

        ImageMargin = new Thickness(marginX, marginY, 0, 0);
        _imgActualMargin = ImageMargin;

        BorderSmallShowSwitch();
        if (_borderMove != null)
        {
            _imgSmallMouseDownMargin = _borderMove.Margin;
            MoveSmallImg(_imgSmallMouseDownMargin.Left, _imgSmallMouseDownMargin.Top);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && _panelMain != null)
        {
            _imgMouseDownPoint = e.GetPosition(_panelMain);
            _imgMouseDownMargin = ImageMargin;
            _imgIsMouseDown = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _imgIsMouseDown = false;
    }

    private void BorderSmallShowSwitch()
    {
        if (!ShowImgMap) return;

        if (_canMoveX || _canMoveY)
        {
            if (!_borderSmallIsLoaded)
            {
                if (_canvasSmallImg != null && _imageMain != null)
                {
                    _canvasSmallImg.Background = new VisualBrush { Visual = _imageMain };
                }
                InitBorderSmall();
                _borderSmallIsLoaded = true;
            }

            ShowSmallImgInternal = true;
            UpdateBorderSmall();
        }
        else
        {
            ShowSmallImgInternal = false;
        }
    }

    private void InitBorderSmall()
    {
        if (_canvasSmallImg == null) return;
        var scaleWindow = _canvasSmallImg.MaxWidth / _canvasSmallImg.MaxHeight;
        if (_imgWidHeiScale > scaleWindow)
        {
            _canvasSmallImg.Width = _canvasSmallImg.MaxWidth;
            _canvasSmallImg.Height = _canvasSmallImg.Width / _imgWidHeiScale;
        }
        else
        {
            _canvasSmallImg.Width = _canvasSmallImg.MaxHeight * _imgWidHeiScale;
            _canvasSmallImg.Height = _canvasSmallImg.MaxHeight;
        }
    }

    private void UpdateBorderSmall()
    {
        if (!ShowSmallImgInternal) return;
        if (_borderMove == null || _canvasSmallImg == null) return;

        var widthMin = Math.Min(ImageWidth, Bounds.Width);
        var heightMin = Math.Min(ImageHeight, Bounds.Height);

        _borderMove.Width = widthMin / ImageWidth * _canvasSmallImg.Width;
        _borderMove.Height = heightMin / ImageHeight * _canvasSmallImg.Height;

        var marginX = -ImageMargin.Left / ImageWidth * _canvasSmallImg.Width;
        var marginY = -ImageMargin.Top / ImageHeight * _canvasSmallImg.Height;

        var marginXMax = _canvasSmallImg.Width - _borderMove.Width;
        var marginYMax = _canvasSmallImg.Height - _borderMove.Height;

        marginX = Math.Max(0, marginX);
        marginX = Math.Min(marginXMax, marginX);
        marginY = Math.Max(0, marginY);
        marginY = Math.Min(marginYMax, marginY);

        _borderMove.Margin = new Thickness(marginX, marginY, 0, 0);
    }

    private void ScaleImg(bool isEnlarge)
    {
        if (_panelMain == null || _borderMove == null || _canvasSmallImg == null) return;
        if (_imgIsMouseDown) return;
        var oldImageWidth = ImageWidth;
        var olgImageHeight = ImageHeight;

        var tempScale = isEnlarge ? _imgActualScale + ScaleInternal : _imgActualScale - ScaleInternal;
        if (Math.Abs(tempScale) < ScaleInternal)
        {
            tempScale = ScaleInternal;
        }
        else if (Math.Abs(tempScale) > 50)
        {
            tempScale = 50;
        }

        ImageScale = tempScale;

        var posCanvas = _imgCurrentPoint;
        var posImg = new Point(posCanvas.X - _imgActualMargin.Left, posCanvas.Y - _imgActualMargin.Top);

        var marginX = .5 * _scaleInternalWidth;
        var marginY = .5 * _scaleInternalHeight;

        if (ImageWidth > Bounds.Width)
        {
            _canMoveX = true;
            if (ImageHeight > Bounds.Height)
            {
                _canMoveY = true;
                marginX = posImg.X / oldImageWidth * _scaleInternalWidth;
                marginY = posImg.Y / olgImageHeight * _scaleInternalHeight;
            }
            else
            {
                _canMoveY = false;
            }
        }
        else
        {
            _canMoveY = ImageHeight > Bounds.Height;
            _canMoveX = false;
        }

        Thickness thickness;
        if (isEnlarge)
        {
            thickness = new Thickness(_imgActualMargin.Left - marginX, _imgActualMargin.Top - marginY, 0, 0);
        }
        else
        {
            var marginActualX = _imgActualMargin.Left + marginX;
            var marginActualY = _imgActualMargin.Top + marginY;
            var subX = ImageWidth - Bounds.Width;
            var subY = ImageHeight - Bounds.Height;

            var right = Math.Abs(_borderMove.Width - _canvasSmallImg.Bounds.Width + _borderMove.Margin.Left);
            var top = Math.Abs(_borderMove.Height - _canvasSmallImg.Bounds.Height + _borderMove.Margin.Top);
            if (Math.Abs(ImageMargin.Left) < 0.001 || right < 0.001)
                marginActualX = _imgActualMargin.Left + _borderMove.Margin.Left /
                    (_canvasSmallImg.Bounds.Width - _borderMove.Width) * _scaleInternalWidth;
            if (Math.Abs(ImageMargin.Top) < 0.001 || top < 0.001)
                marginActualY = _imgActualMargin.Top + _borderMove.Margin.Top /
                    (_canvasSmallImg.Bounds.Height - _borderMove.Height) * _scaleInternalHeight;
            if (subX < 0.001) marginActualX = (Bounds.Width - ImageWidth) / 2;
            if (subY < 0.001) marginActualY = (Bounds.Height - ImageHeight) / 2;
            thickness = new Thickness(marginActualX, marginActualY, 0, 0);
        }

        ImageMargin = thickness;
        _imgActualScale = tempScale;
        _imgActualMargin = thickness;
        BorderSmallShowSwitch();

        _imgSmallMouseDownMargin = _borderMove.Margin;
        MoveSmallImg(_imgSmallMouseDownMargin.Left, _imgSmallMouseDownMargin.Top);
    }

    private void RotateImg(double rotate)
    {
        _imgActualRotate = rotate;

        _isOblique = ((int)_imgActualRotate - 90) % 180 == 0;
        ShowSmallImgInternal = false;
        Init();
        InitBorderSmall();

        BeginAnimation(this, ImageRotateProperty, rotate, 300);
    }

    private void MoveImg(PointerEventArgs e)
    {
        if (_panelMain == null || _borderMove == null || _canvasSmallImg == null) return;

        _imgCurrentPoint = e.GetPosition(_panelMain);
        ShowCloseButton = _imgCurrentPoint.Y < 200;
        ShowBorderBottom = _imgCurrentPoint.Y > Bounds.Height - 200;

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (_imgIsMouseDown)
        {
            var subX = _imgCurrentPoint.X - _imgMouseDownPoint.X;
            var subY = _imgCurrentPoint.Y - _imgMouseDownPoint.Y;

            var marginX = _imgMouseDownMargin.Left;
            if (ImageWidth > Bounds.Width)
            {
                marginX = _imgMouseDownMargin.Left + subX;
                if (marginX >= 0)
                    marginX = 0;
                else if (-marginX + Bounds.Width >= ImageWidth) marginX = Bounds.Width - ImageWidth;
                _canMoveX = true;
            }

            var marginY = _imgMouseDownMargin.Top;
            if (ImageHeight > Bounds.Height)
            {
                marginY = _imgMouseDownMargin.Top + subY;
                if (marginY >= 0)
                    marginY = 0;
                else if (-marginY + Bounds.Height >= ImageHeight) marginY = Bounds.Height - ImageHeight;
                _canMoveY = true;
            }

            ImageMargin = new Thickness(marginX, marginY, 0, 0);
            _imgActualMargin = ImageMargin;

            UpdateBorderSmall();
        }
    }

    private void MoveSmallImg()
    {
        if (!_imgSmallIsMouseDown) return;
        if (_borderMove == null || _canvasSmallImg == null) return;

        _imgSmallCurrentPoint = _lastPosInCanvas;

        var subX = _imgSmallCurrentPoint.X - _imgSmallMouseDownPoint.X;
        var subY = _imgSmallCurrentPoint.Y - _imgSmallMouseDownPoint.Y;

        var marginX = _imgSmallMouseDownMargin.Left + subX;
        var marginY = _imgSmallMouseDownMargin.Top + subY;

        MoveSmallImg(marginX, marginY);
    }

    private void MoveSmallImg(double marginX, double marginY)
    {
        if (_borderMove == null || _canvasSmallImg == null) return;

        if (marginX < 0)
            marginX = 0;
        else if (marginX + _borderMove.Width >= _canvasSmallImg.Width)
            marginX = _canvasSmallImg.Width - _borderMove.Width;
        if (marginY < 0)
            marginY = 0;
        else if (marginY + _borderMove.Height >= _canvasSmallImg.Height)
            marginY = _canvasSmallImg.Height - _borderMove.Height;
        _borderMove.Margin = new Thickness(marginX, marginY, 0, 0);

        var marginActualX = (Bounds.Width - ImageWidth) / 2;
        var marginActualY = (Bounds.Height - ImageHeight) / 2;

        if (_canMoveX) marginActualX = -marginX / _canvasSmallImg.Width * ImageWidth;
        if (_canMoveY) marginActualY = -marginY / _canvasSmallImg.Height * ImageHeight;

        ImageMargin = new Thickness(marginActualX, marginActualY, 0, 0);
        _imgActualMargin = ImageMargin;
    }

    private void CanvasSmallImg_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_borderMove == null || _canvasSmallImg == null) return;
        _imgSmallMouseDownPoint = e.GetPosition(_canvasSmallImg);
        _imgSmallMouseDownMargin = _borderMove.Margin;
        _imgSmallIsMouseDown = true;
        e.Handled = true;
    }

    private void CanvasSmallImg_OnPointerReleased(object? sender, PointerReleasedEventArgs e) => _imgSmallIsMouseDown = false;

    private void CanvasSmallImg_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_canvasSmallImg != null)
        {
            _lastPosInCanvas = e.GetPosition(_canvasSmallImg);
        }
        MoveSmallImg();
    }

    private void OnUriChanged(Uri? newValue)
    {
        ImageSource = newValue is not null ? GetBitmap(newValue) : null;
        if (ImageSource is not null && newValue is not null && newValue.IsAbsoluteUri)
        {
            ImgPath = newValue.AbsolutePath;
            if (File.Exists(ImgPath))
            {
                ImgSize = new FileInfo(ImgPath).Length;
            }
        }
        else
        {
            ImgPath = string.Empty;
            ImgSize = 0;
        }
    }

    private static IImage? GetBitmap(Uri source)
    {
        if (!source.IsFile)
        {
            return null;
        }

        try
        {
            return new Bitmap(source.LocalPath);
        }
        catch
        {
            return null;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isLoaded = true;
        Init();
    }

    private static void BeginAnimation(Control target, AvaloniaProperty property, object to, double milliseconds)
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(milliseconds),
            FillMode = FillMode.Forward
        };
        animation.Children.Add(new KeyFrame
        {
            KeyTime = TimeSpan.FromMilliseconds(milliseconds),
            Setters = { new Setter(property, to) }
        });
        _ = animation.RunAsync(target);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                ImageSource = null;
                if (_imageMain != null)
                {
                    _imageMain.Source = null;
                }
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
