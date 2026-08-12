using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using HandyControl.Data;
using HandyControl.Tools;
using HandyControl.Tools.Interop;

namespace HandyControl.Controls;

public class ScreenshotWindow : Avalonia.Controls.Window
{
    #region fields

    private readonly Screenshot _screenshot;

    private VisualBrush _visualPreview = null!;

    private Size _viewboxSize;

    private IImage _imageSource = null!;

    private byte[] _desktopPixels = null!;

    private int _desktopStride;

    private readonly List<Rectangle> _snapRects = new();

    #region const

    private const int IntervalLength = 1;

    private const int IntervalBigLength = 10;

    private const int SnapLength = 4;

    #endregion

    #region IntPtr

    private IntPtr _desktopWindowHandle;

    private IntPtr _mouseOverWindowHandle;

    private IntPtr _screenshotWindowHandle;

    #endregion

    #region status

    private InteropValues.RECT _desktopWindowRect;

    private InteropValues.RECT _targetWindowRect;

    private readonly int[] _flagArr = new int[4];

    private bool _isOut;

    private bool _canDrag;

    private bool _receiveMoveMsg = true;

    private Point _mousePointOld;

    private Point _pointFixed;

    private InteropValues.POINT _pointFloating;

    private bool _saveScreenshot;

    #endregion

    #endregion

    #region Elements

    internal Panel Canvas { get; set; } = null!;

    internal Rectangle MaskAreaLeft { get; set; } = null!;

    internal Rectangle MaskAreaTop { get; set; } = null!;

    internal Rectangle MaskAreaRight { get; set; } = null!;

    internal Rectangle MaskAreaBottom { get; set; } = null!;

    internal Border TargetArea { get; set; } = null!;

    private Border _magnifier = new();

    #endregion

    #region const

    private const string ElementCanvas = "PART_Canvas";

    private const string ElementMaskAreaLeft = "PART_MaskAreaLeft";

    private const string ElementMaskAreaTop = "PART_MaskAreaTop";

    private const string ElementMaskAreaRight = "PART_MaskAreaRight";

    private const string ElementMaskAreaBottom = "PART_MaskAreaBottom";

    private const string ElementTargetArea = "PART_TargetArea";

    private const string ElementMagnifier = "PART_Magnifier";

    #endregion

    #region prop

    public static readonly StyledProperty<bool> IsDrawingProperty =
        AvaloniaProperty.Register<ScreenshotWindow, bool>(nameof(IsDrawing));

    public bool IsDrawing
    {
        get => GetValue(IsDrawingProperty);
        internal set => SetValue(IsDrawingProperty, value);
    }

    public static readonly StyledProperty<bool> IsSelectingProperty =
        AvaloniaProperty.Register<ScreenshotWindow, bool>(nameof(IsSelecting));

    public bool IsSelecting
    {
        get => GetValue(IsSelectingProperty);
        internal set => SetValue(IsSelectingProperty, value);
    }

    public static readonly StyledProperty<Size> SizeProperty =
        AvaloniaProperty.Register<ScreenshotWindow, Size>(nameof(Size));

    public Size Size
    {
        get => GetValue(SizeProperty);
        internal set => SetValue(SizeProperty, value);
    }

    public static readonly StyledProperty<string?> SizeStrProperty =
        AvaloniaProperty.Register<ScreenshotWindow, string?>(nameof(SizeStr));

    public string? SizeStr
    {
        get => GetValue(SizeStrProperty);
        internal set => SetValue(SizeStrProperty, value);
    }

    public static readonly StyledProperty<Color> PixelColorProperty =
        AvaloniaProperty.Register<ScreenshotWindow, Color>(nameof(PixelColor));

    public Color PixelColor
    {
        get => GetValue(PixelColorProperty);
        internal set => SetValue(PixelColorProperty, value);
    }

    public static readonly StyledProperty<string?> PixelColorStrProperty =
        AvaloniaProperty.Register<ScreenshotWindow, string?>(nameof(PixelColorStr));

    public string? PixelColorStr
    {
        get => GetValue(PixelColorStrProperty);
        internal set => SetValue(PixelColorStrProperty, value);
    }

    public static readonly StyledProperty<Brush?> PreviewBrushProperty =
        AvaloniaProperty.Register<ScreenshotWindow, Brush?>(nameof(PreviewBrush));

    public Brush? PreviewBrush
    {
        get => GetValue(PreviewBrushProperty);
        set => SetValue(PreviewBrushProperty, value);
    }

    #endregion

    public ScreenshotWindow(Screenshot screenshot)
    {
        _screenshot = screenshot;
        DataContext = this;

        WindowDecorations = WindowDecorations.None;
        WindowState = WindowState.Maximized;
        Topmost = true;
        CanResize = false;
        Background = Brushes.Transparent;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        ShowInTaskbar = false;

        AddHandler(InputElement.PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(InputElement.PointerReleasedEvent, OnPreviewPointerReleased, RoutingStrategies.Tunnel);
        AddHandler(InputElement.PointerMovedEvent, OnPreviewPointerMoved, RoutingStrategies.Tunnel);

        Closed += ScreenshotWindow_Closed;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Canvas = e.NameScope.Find<Panel>(ElementCanvas) ?? new Panel();
        MaskAreaLeft = e.NameScope.Find<Rectangle>(ElementMaskAreaLeft) ?? new Rectangle();
        MaskAreaTop = e.NameScope.Find<Rectangle>(ElementMaskAreaTop) ?? new Rectangle();
        MaskAreaRight = e.NameScope.Find<Rectangle>(ElementMaskAreaRight) ?? new Rectangle();
        MaskAreaBottom = e.NameScope.Find<Rectangle>(ElementMaskAreaBottom) ?? new Rectangle();
        TargetArea = e.NameScope.Find<Border>(ElementTargetArea) ?? new Border();
        _magnifier = e.NameScope.Find<Border>(ElementMagnifier) ?? new Border();
        _viewboxSize = new Size(29, 21);

        _snapRects.Clear();
        for (var i = 1; i <= 8; i++)
        {
            var rect = e.NameScope.Find<Rectangle>($"PART_Snap{i}");
            if (rect != null)
            {
                _snapRects.Add(rect);
            }
        }

        _visualPreview = new VisualBrush
        {
            Visual = Canvas,
            Stretch = Stretch.Fill
        };
        PreviewBrush = _visualPreview;
        _magnifier.IsVisible = true;
    }

    private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        if (e.ClickCount >= 2)
        {
            _saveScreenshot = true;
            Close();
            return;
        }

        _mousePointOld = e.GetPosition(this);
    }

    private void OnPreviewPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _magnifier.IsVisible = false;
    }

    private void OnPreviewPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            UpdateStatus(e.GetPosition(TargetArea));
            return;
        }

        var newPoint = e.GetPosition(this);
        var offsetX = (int) (newPoint.X - _mousePointOld.X);
        var offsetY = (int) (newPoint.Y - _mousePointOld.Y);

        if (IsDrawing)
        {
            if (_isOut) return;
            var rect = _targetWindowRect;

            if (_canDrag)
            {
                rect.Left += offsetX;
                rect.Top += offsetY;
                rect.Right += offsetX;
                rect.Bottom += offsetY;
            }
            else
            {
                var magnifierPos = new InteropValues.POINT((int) newPoint.X, (int) newPoint.Y);

                if (_flagArr[0] > 0)
                {
                    _pointFloating.X += offsetX * _flagArr[0];
                    magnifierPos.X = _pointFloating.X;
                }
                else if (_flagArr[2] > 0)
                {
                    _pointFloating.X += offsetX * _flagArr[2];
                    magnifierPos.X = _pointFloating.X - 1;
                }

                if (_flagArr[1] > 0)
                {
                    _pointFloating.Y += offsetY * _flagArr[1];
                    magnifierPos.Y = _pointFloating.Y;
                }
                else if (_flagArr[3] > 0)
                {
                    _pointFloating.Y += offsetY * _flagArr[3];
                    magnifierPos.Y = _pointFloating.Y - 1;
                }

                rect.Left = (int) Math.Min(_pointFixed.X, _pointFloating.X);
                rect.Top = (int) Math.Min(_pointFixed.Y, _pointFloating.Y);
                rect.Right = (int) Math.Max(_pointFixed.X, _pointFloating.X);
                rect.Bottom = (int) Math.Max(_pointFixed.Y, _pointFloating.Y);

                _magnifier.IsVisible = true;
                MoveMagnifier(magnifierPos);
            }

            MoveTargetArea(rect);
            _mousePointOld = newPoint;
        }
        else if (IsSelecting)
        {
            var minX = (int) Math.Min(_mousePointOld.X, newPoint.X);
            var maxX = (int) Math.Max(_mousePointOld.X, newPoint.X);
            var minY = (int) Math.Min(_mousePointOld.Y, newPoint.Y);
            var maxY = (int) Math.Max(_mousePointOld.Y, newPoint.Y);

            MoveTargetArea(new InteropValues.RECT(minX, minY, maxX, maxY));
        }
        else if (!IsSelecting && offsetX > 0 && offsetY > 0)
        {
            IsSelecting = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape)
        {
            Close();
            return;
        }

        if (!IsDrawing) return;

        var moveLength = (e.KeyModifiers & KeyModifiers.Control) != 0
            ? IntervalBigLength
            : IntervalLength;

        switch (e.Key)
        {
            case Key.Left:
                MoveTargetArea(MoveRect(_targetWindowRect, moveLength, leftFlag: -1, rightFlag: -1));
                break;
            case Key.Up:
                MoveTargetArea(MoveRect(_targetWindowRect, moveLength, bottomFlag: -1, topFlag: -1));
                break;
            case Key.Right:
                MoveTargetArea(MoveRect(_targetWindowRect, moveLength, rightFlag: 1));
                break;
            case Key.Down:
                MoveTargetArea(MoveRect(_targetWindowRect, moveLength, bottomFlag: 1, topFlag: 1));
                break;
            case Key.Enter:
                _saveScreenshot = true;
                Close();
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectingProperty || change.Property == IsDrawingProperty)
        {
            UpdateTargetAreaState();
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        _screenshotWindowHandle = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (_screenshotWindowHandle != IntPtr.Zero)
        {
            InteropMethods.EnableWindow(_screenshotWindowHandle, false);
        }

        CaptureDesktop();
        StartHooks();
        InteropMethods.GetCursorPos(out var point);
        MoveElement(point);
        MoveMagnifier(point);
    }

    private void UpdateTargetAreaState()
    {
        if (TargetArea == null) return;

        var visible = IsSelecting || IsDrawing;
        TargetArea.BorderThickness = new Thickness(visible ? 1 : 5);
        TargetArea.IsHitTestVisible = IsDrawing;
        foreach (var rect in _snapRects)
        {
            rect.IsVisible = visible;
        }
    }

    private void ScreenshotWindow_Closed(object? sender, EventArgs e)
    {
        if (_saveScreenshot)
        {
            SaveScreenshot();
        }

        StopHooks();
        IsDrawing = false;

        Closed -= ScreenshotWindow_Closed;
    }

    private void UpdateStatus(Point point)
    {
        Cursor cursor;

        var leftAbs = Math.Abs(point.X);
        var topAbs = Math.Abs(point.Y);
        var rightAbs = Math.Abs(point.X - _targetWindowRect.Width);
        var downAbs = Math.Abs(point.Y - _targetWindowRect.Height);

        _canDrag = false;
        _isOut = false;
        _flagArr[0] = 0;
        _flagArr[1] = 0;
        _flagArr[2] = 0;
        _flagArr[3] = 0;

        if (leftAbs <= SnapLength)
        {
            if (topAbs > SnapLength)
            {
                if (downAbs > SnapLength)
                {
                    cursor = new Cursor(StandardCursorType.SizeWestEast);
                    _pointFixed = new Point(_targetWindowRect.Right, _targetWindowRect.Top);
                    _pointFloating = new InteropValues.POINT(_targetWindowRect.Left, _targetWindowRect.Bottom);
                    _flagArr[0] = 1;
                }
                else
                {
                    cursor = new Cursor(StandardCursorType.TopRightCorner);
                    _pointFixed = new Point(_targetWindowRect.Right, _targetWindowRect.Top);
                    _pointFloating = new InteropValues.POINT(_targetWindowRect.Left, _targetWindowRect.Bottom);
                    _flagArr[0] = 1;
                    _flagArr[3] = 1;
                }
            }
            else
            {
                cursor = new Cursor(StandardCursorType.TopLeftCorner);
                _pointFixed = new Point(_targetWindowRect.Right, _targetWindowRect.Bottom);
                _pointFloating = new InteropValues.POINT(_targetWindowRect.Left, _targetWindowRect.Top);
                _flagArr[0] = 1;
                _flagArr[1] = 1;
            }
        }
        else if (rightAbs > SnapLength)
        {
            if (topAbs > SnapLength)
            {
                if (downAbs > SnapLength)
                {
                    if (TargetArea.IsPointerOver)
                    {
                        cursor = new Cursor(StandardCursorType.SizeAll);
                        _canDrag = true;
                    }
                    else
                    {
                        cursor = new Cursor(StandardCursorType.Arrow);
                        _isOut = true;
                    }
                }
                else
                {
                    cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                    _pointFixed = new Point(_targetWindowRect.Left, _targetWindowRect.Top);
                    _pointFloating = new InteropValues.POINT(_targetWindowRect.Right, _targetWindowRect.Bottom);
                    _flagArr[3] = 1;
                }
            }
            else
            {
                cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                _pointFixed = new Point(_targetWindowRect.Right, _targetWindowRect.Bottom);
                _pointFloating = new InteropValues.POINT(_targetWindowRect.Left, _targetWindowRect.Top);
                _flagArr[1] = 1;
            }
        }
        else if (rightAbs <= SnapLength)
        {
            if (topAbs > SnapLength)
            {
                if (downAbs > SnapLength)
                {
                    cursor = new Cursor(StandardCursorType.SizeWestEast);
                    _pointFixed = new Point(_targetWindowRect.Left, _targetWindowRect.Bottom);
                    _pointFloating = new InteropValues.POINT(_targetWindowRect.Right, _targetWindowRect.Top);
                    _flagArr[2] = 1;
                }
                else
                {
                    cursor = new Cursor(StandardCursorType.TopLeftCorner);
                    _pointFixed = new Point(_targetWindowRect.Left, _targetWindowRect.Top);
                    _pointFloating = new InteropValues.POINT(_targetWindowRect.Right, _targetWindowRect.Bottom);
                    _flagArr[2] = 1;
                    _flagArr[3] = 1;
                }
            }
            else
            {
                cursor = new Cursor(StandardCursorType.TopRightCorner);
                _pointFixed = new Point(_targetWindowRect.Left, _targetWindowRect.Bottom);
                _pointFloating = new InteropValues.POINT(_targetWindowRect.Right, _targetWindowRect.Top);
                _flagArr[1] = 1;
                _flagArr[2] = 1;
            }
        }
        else
        {
            cursor = new Cursor(StandardCursorType.Arrow);
            _isOut = true;
        }

        TargetArea.Cursor = cursor;
    }

    private void StopHooks()
    {
        MouseHook.Stop();
        MouseHook.StatusChanged -= MouseHook_StatusChanged;
    }

    private void StartHooks()
    {
        MouseHook.Start();
        MouseHook.StatusChanged += MouseHook_StatusChanged;
    }

    private void MouseHook_StatusChanged(object? sender, MouseHookEventArgs e)
    {
        switch (e.MessageType)
        {
            case MouseHookMessageType.MouseMove:
                MoveElement(e.Point);
                MoveMagnifier(e.Point);
                break;
            case MouseHookMessageType.LeftButtonDown:
                _receiveMoveMsg = false;
                _mousePointOld = new Point(e.Point.X, e.Point.Y);
                if (_screenshotWindowHandle != IntPtr.Zero)
                {
                    InteropMethods.EnableWindow(_screenshotWindowHandle, true);
                }
                break;
            case MouseHookMessageType.RightButtonDown:
                if (!IsDrawing) Close();
                break;
            case MouseHookMessageType.LeftButtonUp:
                StopHooks();
                IsSelecting = false;
                IsDrawing = true;
                _magnifier.IsVisible = false;
                break;
        }
    }

    private void SaveScreenshot()
    {
        var left = _targetWindowRect.Left;
        var top = _targetWindowRect.Top;
        var width = _targetWindowRect.Width;
        var height = _targetWindowRect.Height;

        var cropped = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        {
            Array.Copy(_desktopPixels, (top + y) * _desktopStride + left * 4, cropped, y * width * 4, width * 4);
        }

        var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormats.Bgra8888,
            AlphaFormat.Premul);
        using (var fb = bitmap.Lock())
        {
            var ptr = fb.Address;
            for (var y = 0; y < height; y++)
            {
                Marshal.Copy(cropped, y * width * 4, ptr + y * fb.RowBytes, width * 4);
            }
        }

        _screenshot.OnSnapped(bitmap);
        Close();
    }

    private void CaptureDesktop()
    {
        _desktopWindowHandle = InteropMethods.GetDesktopWindow();
        var hdcSrc = InteropMethods.GetWindowDC(_desktopWindowHandle);
        var hdcDest = InteropMethods.CreateCompatibleDC(hdcSrc);

        InteropMethods.GetWindowRect(_desktopWindowHandle, out _desktopWindowRect);
        var desktopWindowWidth = _desktopWindowRect.Width;
        var desktopWindowHeight = _desktopWindowRect.Height;

        var hbitmap = InteropMethods.CreateCompatibleBitmap(hdcSrc, desktopWindowWidth, desktopWindowHeight);
        var hOld = InteropMethods.SelectObject(hdcDest, hbitmap);
        InteropMethods.BitBlt(hdcDest, 0, 0, desktopWindowWidth, desktopWindowHeight, hdcSrc, 0, 0,
            InteropValues.SRCCOPY);

        _desktopStride = desktopWindowWidth * 4;

        var bmi = new InteropValues.BITMAPINFO();
        bmi.bmiHeader.biSize = (uint) Marshal.SizeOf(typeof(InteropValues.BITMAPINFOHEADER));
        bmi.bmiHeader.biWidth = desktopWindowWidth;
        bmi.bmiHeader.biHeight = -desktopWindowHeight;
        bmi.bmiHeader.biPlanes = 1;
        bmi.bmiHeader.biBitCount = 32;
        bmi.bmiHeader.biCompression = InteropValues.BI_RGB;

        var buffer = Marshal.AllocHGlobal(_desktopStride * desktopWindowHeight);
        try
        {
            InteropMethods.GetDIBits(hdcDest, hbitmap, 0, (uint) desktopWindowHeight, buffer, ref bmi,
                InteropValues.DIB_RGB_COLORS);
            _desktopPixels = new byte[_desktopStride * desktopWindowHeight];
            Marshal.Copy(buffer, _desktopPixels, 0, _desktopPixels.Length);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        InteropMethods.SelectObject(hdcDest, hOld);
        InteropMethods.DeleteObject(hbitmap);
        InteropMethods.DeleteDC(hdcDest);
        InteropMethods.ReleaseDC(_desktopWindowHandle, hdcSrc);

        _imageSource = CreateBitmap(_desktopPixels, desktopWindowWidth, desktopWindowHeight);

        var image = new Image
        {
            Source = _imageSource,
            Stretch = Stretch.None
        };
        RenderOptions.SetBitmapInterpolationMode(image, BitmapInterpolationMode.None);
        Canvas.Children.Add(image);
    }

    private static IImage CreateBitmap(byte[] pixels, int width, int height)
    {
        var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormats.Bgra8888,
            AlphaFormat.Premul);
        using (var fb = bitmap.Lock())
        {
            var ptr = fb.Address;
            for (var y = 0; y < height; y++)
            {
                Marshal.Copy(pixels, y * width * 4, ptr + y * fb.RowBytes, width * 4);
            }
        }
        return bitmap;
    }

    private void MoveElement(InteropValues.POINT point)
    {
        if (!_receiveMoveMsg) return;

        var mouseOverWindowHandle = InteropMethods.ChildWindowFromPointEx(_desktopWindowHandle,
            new InteropValues.POINT
            {
                X = point.X,
                Y = point.Y
            }, 1 | 2);

        if (mouseOverWindowHandle != _mouseOverWindowHandle && mouseOverWindowHandle != IntPtr.Zero)
        {
            _mouseOverWindowHandle = mouseOverWindowHandle;

            InteropMethods.GetWindowRect(_mouseOverWindowHandle, out var windowRect);
            MoveTargetArea(windowRect);
        }
    }

    private static InteropValues.RECT MoveRect(InteropValues.RECT rect, int moveLength, int leftFlag = 0, int topFlag = 0, int rightFlag = 0, int bottomFlag = 0)
    {
        rect.Left += leftFlag * moveLength;
        rect.Top += topFlag * moveLength;
        rect.Right += rightFlag * moveLength;
        rect.Bottom += bottomFlag * moveLength;

        return rect;
    }

    private void MoveTargetArea(InteropValues.RECT rect)
    {
        if (rect.Left < 0)
        {
            rect.Right -= rect.Left;
            rect.Left = 0;
        }

        if (rect.Top < 0)
        {
            rect.Bottom -= rect.Top;
            rect.Top = 0;
        }

        if (rect.Right > _desktopWindowRect.Width)
        {
            rect.Left -= rect.Right - _desktopWindowRect.Width;
            rect.Right = _desktopWindowRect.Width;
        }

        if (rect.Bottom > _desktopWindowRect.Height)
        {
            rect.Top -= rect.Bottom - _desktopWindowRect.Height;
            rect.Bottom = _desktopWindowRect.Height;
        }

        rect.Left = Math.Max(0, rect.Left);
        rect.Top = Math.Max(0, rect.Top);

        var width = rect.Width;
        var height = rect.Height;
        var left = rect.Left;
        var top = rect.Top;

        TargetArea.Width = width;
        TargetArea.Height = height;
        TargetArea.Margin = new Thickness(left, top, 0, 0);

        _targetWindowRect = new InteropValues.RECT(left, top, left + width, top + height);
        Size = _targetWindowRect.Size;
        SizeStr = $"{_targetWindowRect.Width} x {_targetWindowRect.Height}";

        MoveMaskArea();
    }

    private void MoveMaskArea()
    {
        MaskAreaLeft.Width = TargetArea.Margin.Left;
        MaskAreaLeft.Height = _desktopWindowRect.Height;

        MaskAreaTop.Margin = new Thickness(TargetArea.Margin.Left, 0, 0, 0);
        MaskAreaTop.Width = TargetArea.Width;
        MaskAreaTop.Height = TargetArea.Margin.Top;

        MaskAreaRight.Margin = new Thickness(TargetArea.Width + TargetArea.Margin.Left, 0, 0, 0);
        MaskAreaRight.Width = _desktopWindowRect.Width - TargetArea.Margin.Left - TargetArea.Width;
        MaskAreaRight.Height = _desktopWindowRect.Height;

        MaskAreaBottom.Margin = new Thickness(TargetArea.Margin.Left, TargetArea.Height + TargetArea.Margin.Top, 0, 0);
        MaskAreaBottom.Width = TargetArea.Width;
        MaskAreaBottom.Height = _desktopWindowRect.Height - TargetArea.Height - TargetArea.Margin.Top;
    }

    private void MoveMagnifier(InteropValues.POINT point)
    {
        if (_visualPreview == null || _magnifier == null) return;

        _magnifier.Margin = new Thickness(point.X + 4, point.Y + 26, 0, 0);
        _visualPreview.SourceRect = new RelativeRect(
            new Rect(point.X - _viewboxSize.Width / 2 + 0.5, point.Y - _viewboxSize.Height / 2 + 0.5,
                _viewboxSize.Width, _viewboxSize.Height), RelativeUnit.Absolute);
    }
}