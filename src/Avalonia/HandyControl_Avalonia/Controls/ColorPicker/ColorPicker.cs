using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using HandyControl.Data;
using HandyControl.Tools.Extension;

namespace HandyControl.Controls;

/// <summary>
///     颜色拾取器
/// </summary>
public class ColorPicker : TemplatedControl
{
    #region Constants

    private const string ElementBorderColor = "PART_BorderColor";
    private const string ElementBorderPicker = "PART_BorderPicker";
    private const string ElementBorderDrag = "PART_BorderDrag";
    private const string ElementPanelColor = "PART_PanelColor";
    private const string ElementSliderColor = "PART_SliderColor";
    private const string ElementSliderOpacity = "PART_SliderOpacity";
    private const string ElementPanelRgb = "PART_PanelRgb";
    private const string ElementPanelHex = "PART_PanelHex";

    #endregion Constants

    #region Data

    private Panel? _panelRgb;
    private Panel? _panelHex;
    private Border? _borderColor;
    private Border? _borderPicker;
    private Border? _borderDrag;
    private Panel? _panelColor;
    private Slider? _sliderColor;
    private Slider? _sliderOpacity;
    private NumericUpDown? _rgbR;
    private NumericUpDown? _rgbG;
    private NumericUpDown? _rgbB;
    private NumericUpDown? _rgbA;
    private TextBox? _hexBox;
    private bool _appliedTemplate;
    private bool _isLoaded;
    private bool _isNeedUpdatePicker = true;
    private bool _isOnDragging;
    private int _colorType;

    private bool IsNeedUpdateInfo { get; set; } = true;

    private const double ColorPanelWidth = 230;
    private const double ColorPanelHeight = 122;

    private readonly List<string> _colorPresetList = new()
    {
        "#f44336", "#e91e63", "#9c27b0", "#673ab7", "#3f51b5", "#2196f3", "#03a9f4", "#00bcd4", "#009688",
        "#4caf50", "#8bc34a", "#cddc39", "#ffeb3b", "#ffc107", "#ff9800", "#ff5722", "#795548", "#9e9e9e"
    };

    private readonly List<ColorRange> _colorRangeList = new()
    {
        new ColorRange { Start = Color.FromRgb(255, 0, 0), End = Color.FromRgb(255, 0, 255) },
        new ColorRange { Start = Color.FromRgb(255, 0, 255), End = Color.FromRgb(0, 0, 255) },
        new ColorRange { Start = Color.FromRgb(0, 0, 255), End = Color.FromRgb(0, 255, 255) },
        new ColorRange { Start = Color.FromRgb(0, 255, 255), End = Color.FromRgb(0, 255, 0) },
        new ColorRange { Start = Color.FromRgb(0, 255, 0), End = Color.FromRgb(255, 255, 0) },
        new ColorRange { Start = Color.FromRgb(255, 255, 0), End = Color.FromRgb(255, 0, 0) }
    };

    private readonly List<Color> _colorSeparateList = new()
    {
        Color.FromRgb(255, 0, 0), Color.FromRgb(255, 0, 255), Color.FromRgb(0, 0, 255),
        Color.FromRgb(0, 255, 255), Color.FromRgb(0, 255, 0), Color.FromRgb(255, 255, 0)
    };

    #endregion Data

    #region Events

    public static readonly RoutedEvent<FunctionEventArgs<Color>> SelectedColorChangedEvent =
        RoutedEvent.Register<ColorPicker, FunctionEventArgs<Color>>(nameof(SelectedColorChanged), RoutingStrategies.Bubble);

    public event EventHandler<FunctionEventArgs<Color>> SelectedColorChanged
    {
        add => AddHandler(SelectedColorChangedEvent, value);
        remove => RemoveHandler(SelectedColorChangedEvent, value);
    }

    public static readonly RoutedEvent<FunctionEventArgs<Color>> ConfirmedEvent =
        RoutedEvent.Register<ColorPicker, FunctionEventArgs<Color>>(nameof(Confirmed), RoutingStrategies.Bubble);

    public event EventHandler<FunctionEventArgs<Color>> Confirmed
    {
        add => AddHandler(ConfirmedEvent, value);
        remove => RemoveHandler(ConfirmedEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> CanceledEvent =
        RoutedEvent.Register<ColorPicker, RoutedEventArgs>(nameof(Canceled), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> Canceled
    {
        add => AddHandler(CanceledEvent, value);
        remove => RemoveHandler(CanceledEvent, value);
    }

    #endregion Events

    #region Properties

    internal static readonly StyledProperty<int> ChannelAProperty =
        AvaloniaProperty.Register<ColorPicker, int>(nameof(ChannelA), 255);

    internal int ChannelA
    {
        get => GetValue(ChannelAProperty);
        set => SetValue(ChannelAProperty, value);
    }

    internal static readonly StyledProperty<int> ChannelRProperty =
        AvaloniaProperty.Register<ColorPicker, int>(nameof(ChannelR), 255);

    internal int ChannelR
    {
        get => GetValue(ChannelRProperty);
        set => SetValue(ChannelRProperty, value);
    }

    internal static readonly StyledProperty<int> ChannelGProperty =
        AvaloniaProperty.Register<ColorPicker, int>(nameof(ChannelG), 255);

    internal int ChannelG
    {
        get => GetValue(ChannelGProperty);
        set => SetValue(ChannelGProperty, value);
    }

    internal static readonly StyledProperty<int> ChannelBProperty =
        AvaloniaProperty.Register<ColorPicker, int>(nameof(ChannelB), 255);

    internal int ChannelB
    {
        get => GetValue(ChannelBProperty);
        set => SetValue(ChannelBProperty, value);
    }

    public static readonly StyledProperty<SolidColorBrush?> SelectedBrushProperty =
        AvaloniaProperty.Register<ColorPicker, SolidColorBrush?>(nameof(SelectedBrush), new SolidColorBrush(Colors.White));

    /// <summary>
    ///     当前选中的颜色
    /// </summary>
    public SolidColorBrush? SelectedBrush
    {
        get => GetValue(SelectedBrushProperty);
        set => SetValue(SelectedBrushProperty, value);
    }

    public static readonly StyledProperty<SolidColorBrush?> SelectedBrushWithoutOpacityProperty =
        AvaloniaProperty.Register<ColorPicker, SolidColorBrush?>(nameof(SelectedBrushWithoutOpacity), new SolidColorBrush(Colors.White));

    public SolidColorBrush? SelectedBrushWithoutOpacity
    {
        get => GetValue(SelectedBrushWithoutOpacityProperty);
        internal set => SetValue(SelectedBrushWithoutOpacityProperty, value);
    }

    public static readonly StyledProperty<SolidColorBrush?> BackColorProperty =
        AvaloniaProperty.Register<ColorPicker, SolidColorBrush?>(nameof(BackColor), new SolidColorBrush(Colors.Red));

    public SolidColorBrush? BackColor
    {
        get => GetValue(BackColorProperty);
        internal set => SetValue(BackColorProperty, value);
    }

    #endregion Properties

    static ColorPicker()
    {
        FocusableProperty.OverrideDefaultValue<ColorPicker>(false);
        SelectedBrushProperty.Changed.AddClassHandler<ColorPicker>((c, e) =>
        {
            c.OnSelectedBrushChanged(e.GetNewValue<SolidColorBrush?>());
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (!_isLoaded)
        {
            Init();
            _isLoaded = true;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _appliedTemplate = false;

        if (_borderDrag != null)
        {
            _borderDrag.PointerPressed -= BorderDrag_PointerPressed;
            _borderDrag.PointerMoved -= BorderDrag_PointerMoved;
            _borderDrag.PointerReleased -= BorderDrag_PointerReleased;
        }

        base.OnApplyTemplate(e);

        _borderColor = e.NameScope.Find<Border>(ElementBorderColor);
        _borderDrag = e.NameScope.Find<Border>(ElementBorderDrag);
        _borderPicker = e.NameScope.Find<Border>(ElementBorderPicker);
        _panelColor = e.NameScope.Find<Panel>(ElementPanelColor);
        _sliderColor = e.NameScope.Find<Slider>(ElementSliderColor);
        _sliderOpacity = e.NameScope.Find<Slider>(ElementSliderOpacity);
        _panelRgb = e.NameScope.Find<Panel>(ElementPanelRgb);
        _panelHex = e.NameScope.Find<Panel>(ElementPanelHex);

        if (_borderDrag != null)
        {
            _borderDrag.PointerPressed += BorderDrag_PointerPressed;
            _borderDrag.PointerMoved += BorderDrag_PointerMoved;
            _borderDrag.PointerReleased += BorderDrag_PointerReleased;
        }

        if (e.NameScope.Find<Button>("PART_Confirm") is { } confirmButton)
        {
            confirmButton.Click += ButtonConfirm_OnClick;
        }
        if (e.NameScope.Find<Button>("PART_Cancel") is { } cancelButton)
        {
            cancelButton.Click += ButtonCancel_OnClick;
        }
        if (e.NameScope.Find<Button>("PART_Switch") is { } switchButton)
        {
            switchButton.Click += ButtonSwitch_OnClick;
        }
        _rgbR = e.NameScope.Find<NumericUpDown>("PART_RgbR");
        _rgbG = e.NameScope.Find<NumericUpDown>("PART_RgbG");
        _rgbB = e.NameScope.Find<NumericUpDown>("PART_RgbB");
        _rgbA = e.NameScope.Find<NumericUpDown>("PART_RgbA");
        if (_rgbR != null) _rgbR.ValueChanged += NumericUpDownRgb_OnValueChanged;
        if (_rgbG != null) _rgbG.ValueChanged += NumericUpDownRgb_OnValueChanged;
        if (_rgbB != null) _rgbB.ValueChanged += NumericUpDownRgb_OnValueChanged;
        if (_rgbA != null) _rgbA.ValueChanged += NumericUpDownRgb_OnValueChanged;
        _hexBox = e.NameScope.Find<TextBox>("PART_HexBox");
        if (_hexBox != null) _hexBox.LostFocus += HexBox_OnLostFocus;

        _appliedTemplate = true;
        if (_isLoaded)
        {
            Init();
        }
    }

    /// <summary>
    ///     初始化
    /// </summary>
    private void Init()
    {
        if (_panelColor == null) return;
        if (_appliedTemplate)
        {
            UpdateStatus(SelectedBrush?.Color ?? Colors.Black);
        }

        _panelColor.Children.Clear();
        foreach (var item in _colorPresetList)
        {
            _panelColor.Children.Add(CreateColorButton(item));
        }

        UpdatePanels();
    }

    /// <summary>
    ///     创建颜色按钮
    /// </summary>
    private Button CreateColorButton(string colorStr)
    {
        var color = Color.Parse(colorStr);
        var brush = new SolidColorBrush(color);

        var button = new Button
        {
            Margin = new Thickness(6),
            Content = new Border
            {
                Background = brush,
                Width = 12,
                Height = 12,
                CornerRadius = new CornerRadius(2)
            }
        };

        button.Click += (_, _) =>
        {
            SelectedBrush = brush;
            SetCurrentValue(ChannelAProperty, byte.MaxValue);
            UpdateHexText();
            UpdateRgbPanels();
        };

        return button;
    }

    /// <summary>
    ///     内部更新
    /// </summary>
    private void UpdateStatus(Color color)
    {
        if (_isOnDragging || _sliderColor == null) return;

        var r = color.R;
        var g = color.G;
        var b = color.B;
        var list = new List<byte> { r, g, b };

        var max = list.Max();
        var min = list.Min();

        if (min == max)
        {
            if (r == g && b == g)
            {
                BackColor = new SolidColorBrush(Colors.Red);
                IsNeedUpdateInfo = false;
                if (!_sliderColor.IsPointerOver && !_sliderOpacity!.IsPointerOver)
                {
                    _sliderColor.Value = 0;
                }
                IsNeedUpdateInfo = true;
            }
        }
        else
        {
            var maxIndex = list.IndexOf(max);
            var minIndex = list.IndexOf(min);
            var commonIndex = 3 - maxIndex - minIndex;
            if (commonIndex == 3)
            {
                BackColor = new SolidColorBrush(Colors.Red);
                IsNeedUpdateInfo = false;
                if (!_sliderColor.IsPointerOver && !_sliderOpacity!.IsPointerOver)
                {
                    _sliderColor.Value = 0;
                }
                IsNeedUpdateInfo = true;
            }
            else
            {
                var common = list[commonIndex];
                list[maxIndex] = 255;
                list[minIndex] = 0;
                common = (byte)(255 * (min - common) / (double)(min - max));
                list[commonIndex] = common;
                BackColor = new SolidColorBrush(Color.FromRgb(list[0], list[1], list[2]));

                list[commonIndex] = 0;
                var cIndex = _colorSeparateList.IndexOf(Color.FromRgb(list[0], list[1], list[2]));
                int sub;
                var direc = 0;
                if (cIndex is < 5 and > 0)
                {
                    var nextColorList = _colorSeparateList[cIndex + 1].ToList();
                    var prevColorList = _colorSeparateList[cIndex - 1].ToList();
                    if (nextColorList[minIndex] > 0)
                    {
                        var target = prevColorList[commonIndex];
                        direc = 1;
                        sub = target - common;
                    }
                    else
                    {
                        sub = common;
                    }
                }
                else if (cIndex == 0)
                {
                    sub = common;
                    if (minIndex == 2)
                    {
                        sub = 255 - common;
                        direc = -5;
                    }
                }
                else
                {
                    sub = 255 - common;
                }
                var scale = sub / 255.0;
                var scaleTotal = cIndex - direc + scale;
                IsNeedUpdateInfo = false;
                if (!_sliderColor.IsPointerOver && !_sliderOpacity!.IsPointerOver)
                {
                    _sliderColor.Value = scaleTotal;
                }
                IsNeedUpdateInfo = true;
            }
        }

        if (_borderPicker == null) return;
        var x = max == 0 ? 0 : (1 - min / (double)max) * ColorPanelWidth;
        var y = (1 - max / 255.0) * ColorPanelHeight;
        if (_isNeedUpdatePicker)
        {
            _borderPicker.RenderTransform = new TranslateTransform(x, y);
        }
    }

    private void SliderColor_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (!_appliedTemplate || !IsNeedUpdateInfo) return;
        var index = Math.Min(5, (int)Math.Floor(e.NewValue));
        var sub = e.NewValue - index;
        var range = _colorRangeList[index];

        var color = range.GetColor(sub);
        BackColor = new SolidColorBrush(color);

        var offset = GetPickerOffset();
        _isNeedUpdatePicker = false;
        UpdateColorWhenDrag(new Point(offset.X, offset.Y));
        _isNeedUpdatePicker = true;
    }

    private void SliderOpacity_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (!_appliedTemplate || !IsNeedUpdateInfo) return;
        var color = SelectedBrush?.Color ?? Colors.Black;
        SelectedBrush = new SolidColorBrush(Color.FromArgb((byte)(_sliderOpacity?.Value ?? 255), color.R, color.G, color.B));
        UpdateHexText();
    }

    private void BorderDrag_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_borderColor == null || _borderDrag == null) return;
        var point = e.GetCurrentPoint(_borderDrag);
        if (!point.Properties.IsLeftButtonPressed) return;

        e.Pointer.Capture(_borderDrag);
        _isOnDragging = true;
        UpdateColorWhenDrag(e.GetPosition(_borderColor));
        _isOnDragging = false;
    }

    private void BorderDrag_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_borderColor == null || !_isOnDragging) return;
        if (e.GetCurrentPoint(_borderDrag).Properties.IsLeftButtonPressed)
        {
            _isOnDragging = true;
            UpdateColorWhenDrag(e.GetPosition(_borderColor));
            _isOnDragging = false;
        }
    }

    private void BorderDrag_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isOnDragging = false;
        e.Pointer.Capture(null);
    }

    /// <summary>
    ///     拖拽时更新颜色
    /// </summary>
    private void UpdateColorWhenDrag(Point p)
    {
        if (_borderDrag == null) return;

        if (p.X < 0) p = p.WithX(0);
        else if (p.X > ColorPanelWidth) p = new Point(ColorPanelWidth, p.Y);

        if (p.Y < 0) p = new Point(p.X, 0);
        else if (p.Y > ColorPanelHeight) p = new Point(p.X, ColorPanelHeight);

        if (_borderPicker != null && _isNeedUpdatePicker)
        {
            _borderPicker.RenderTransform = new TranslateTransform(p.X, p.Y);
        }

        var scaleX = p.X / ColorPanelWidth;
        var scaleY = 1 - p.Y / ColorPanelHeight;

        var colorYLeft = Color.FromRgb((byte)(255 * scaleY), (byte)(255 * scaleY), (byte)(255 * scaleY));
        var backColor = (BackColor ?? new SolidColorBrush(Colors.Black)).Color;
        var colorYRight = Color.FromRgb((byte)(backColor.R * scaleY), (byte)(backColor.G * scaleY), (byte)(backColor.B * scaleY));

        var subR = colorYLeft.R - colorYRight.R;
        var subG = colorYLeft.G - colorYRight.G;
        var subB = colorYLeft.B - colorYRight.B;

        var alpha = (byte)(_sliderOpacity?.Value ?? 1);
        var color = Color.FromArgb(alpha, (byte)(colorYLeft.R - subR * scaleX),
            (byte)(colorYLeft.G - subG * scaleX), (byte)(colorYLeft.B - subB * scaleX));
        SelectedBrush = new SolidColorBrush(color);
        UpdateHexText();
    }

    private Point GetPickerOffset()
    {
        if (_borderPicker?.RenderTransform is TranslateTransform tt)
        {
            return new Point(tt.X, tt.Y);
        }
        return new Point(0, 0);
    }

    private void ButtonSwitch_OnClick(object? sender, RoutedEventArgs e) => SwitchPanel();

    private void SwitchPanel()
    {
        _colorType++;
        if (_colorType > 1) _colorType = 0;
        UpdatePanels();
    }

    private void UpdatePanels()
    {
        if (_panelHex == null || _panelRgb == null) return;
        _panelHex.IsVisible = _colorType == 0;
        _panelRgb.IsVisible = _colorType == 1;
    }

    private void UpdateHexText()
    {
        if (_hexBox == null || !_appliedTemplate) return;
        var color = SelectedBrush?.Color ?? Colors.Black;
        _hexBox.Text = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private void UpdateRgbPanels()
    {
        if (_panelRgb == null) return;
        var color = SelectedBrush?.Color ?? Colors.Black;
        var nums = new List<NumericUpDown> { _rgbR!, _rgbG!, _rgbB!, _rgbA! };
        UpdateChannelsToNumeric(nums);
    }

    private void UpdateChannelsToNumeric(List<NumericUpDown> nums)
    {
        var color = SelectedBrush?.Color ?? Colors.Black;
        foreach (var num in nums)
        {
            var tag = num.Tag as string;
            var val = tag switch
            {
                "R" => color.R,
                "G" => color.G,
                "B" => color.B,
                _ => color.A
            };
            num.Value = val;
        }
    }

    private void NumericUpDownRgb_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (!_appliedTemplate || !IsNeedUpdateInfo) return;
        if (sender is not NumericUpDown num || num.Tag is not string tag || e.NewValue is not { } newValue) return;

        IsNeedUpdateInfo = false;
        var color = SelectedBrush?.Color ?? Colors.Black;
        var value = (byte)newValue;
        SelectedBrush = tag switch
        {
            "R" => new SolidColorBrush(Color.FromArgb(color.A, value, color.G, color.B)),
            "G" => new SolidColorBrush(Color.FromArgb(color.A, color.R, value, color.B)),
            "B" => new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, value)),
            _ => new SolidColorBrush(Color.FromArgb(value, color.R, color.G, color.B))
        };
        IsNeedUpdateInfo = true;
    }

    private void HexBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox box) return;
        if (Color.TryParse(box.Text, out var color))
        {
            SelectedBrush = new SolidColorBrush(Color.FromArgb(SelectedBrush?.Color.A ?? 255, color.R, color.G, color.B));
        }
        UpdateHexText();
    }

    private void OnSelectedBrushChanged(SolidColorBrush? newValue)
    {
        var color = newValue?.Color ?? Colors.Black;

        if (IsNeedUpdateInfo)
        {
            IsNeedUpdateInfo = false;
            ChannelR = color.R;
            ChannelG = color.G;
            ChannelB = color.B;
            ChannelA = color.A;
            IsNeedUpdateInfo = true;
        }
        UpdateStatus(color);
        SelectedBrushWithoutOpacity = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
        RaiseEvent(new FunctionEventArgs<Color>(SelectedColorChangedEvent, this) { Info = color });
    }

    private void ButtonConfirm_OnClick(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new FunctionEventArgs<Color>(ConfirmedEvent, SelectedBrush?.Color ?? Colors.Black) { Info = SelectedBrush?.Color ?? Colors.Black });
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(CanceledEvent));
}