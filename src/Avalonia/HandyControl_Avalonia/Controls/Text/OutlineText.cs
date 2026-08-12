using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using HandyControl.Data;
using HandyControl.Tools.Helper;

namespace HandyControl.Controls;

public class OutlineText : Control
{
    private Pen? _pen;
    private FormattedText? _formattedText;
    private Geometry? _textGeometry;

    public static readonly StyledProperty<StrokePosition> StrokePositionProperty =
        AvaloniaProperty.Register<OutlineText, StrokePosition>(nameof(StrokePosition));

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<OutlineText, string>(nameof(Text), string.Empty);

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<OutlineText, TextAlignment>(nameof(TextAlignment));

    public static readonly StyledProperty<TextTrimming> TextTrimmingProperty =
        AvaloniaProperty.Register<OutlineText, TextTrimming>(nameof(TextTrimming));

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<OutlineText, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<OutlineText, IBrush?>(nameof(Fill), Brushes.Black);

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<OutlineText, IBrush?>(nameof(Stroke), Brushes.Black);

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<OutlineText, double>(nameof(StrokeThickness), 0.0);

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        AvaloniaProperty.Register<OutlineText, FontFamily>(nameof(FontFamily), FontFamily.Default);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<OutlineText, double>(nameof(FontSize), 12.0);

    public static readonly StyledProperty<FontStretch> FontStretchProperty =
        AvaloniaProperty.Register<OutlineText, FontStretch>(nameof(FontStretch), FontStretch.Normal);

    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        AvaloniaProperty.Register<OutlineText, FontStyle>(nameof(FontStyle), FontStyle.Normal);

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<OutlineText, FontWeight>(nameof(FontWeight), FontWeight.Normal);

    static OutlineText()
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        UpdateFormattedText();
        _textGeometry = null;
        InvalidateMeasure();
        InvalidateVisual();
    }

    public StrokePosition StrokePosition
    {
        get => GetValue(StrokePositionProperty);
        set => SetValue(StrokePositionProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public TextTrimming TextTrimming
    {
        get => GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontStretch FontStretch
    {
        get => GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        if (StrokeThickness > 0)
        {
            EnsureGeometry();
            if (_textGeometry == null) return;

            if (StrokePosition == StrokePosition.Outside)
            {
                using (context.PushGeometryClip(_textGeometry))
                {
                    context.DrawGeometry(null, new Pen(Stroke, StrokeThickness * 2), _textGeometry);
                }
                context.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), _textGeometry);
            }
            else if (StrokePosition == StrokePosition.Inside)
            {
                context.DrawGeometry(Fill, null, _textGeometry);
                using (context.PushGeometryClip(_textGeometry))
                {
                    context.DrawGeometry(null, new Pen(Stroke, StrokeThickness * 2), _textGeometry);
                }
            }
            else
            {
                context.DrawGeometry(Fill, null, _textGeometry);
                context.DrawGeometry(null, _pen, _textGeometry);
            }
        }
        else
        {
            UpdateFormattedText();
            if (_formattedText != null)
            {
                context.DrawText(_formattedText, new Point());
            }
        }
    }

    private void UpdatePen()
    {
        _pen = new Pen(Stroke, StrokeThickness);
    }

    private void EnsureFormattedText()
    {
        if (_formattedText != null || Text == null) return;

        _formattedText = TextHelper.CreateFormattedText(Text, FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize);

        UpdateFormattedText();
    }

    private void EnsureGeometry()
    {
        if (_textGeometry != null) return;

        EnsureFormattedText();
        _textGeometry = _formattedText?.BuildGeometry(new Point(0, 0));
    }

    private void UpdateFormattedText()
    {
        if (_formattedText == null) return;

        _formattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
        _formattedText.TextAlignment = TextAlignment;
        _formattedText.Trimming = TextTrimming;

        _formattedText.SetFontSize(FontSize);
        _formattedText.SetFontStyle(FontStyle);
        _formattedText.SetFontWeight(FontWeight);
        if (Fill != null)
        {
            _formattedText.SetForegroundBrush(Fill);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureFormattedText();
        if (_formattedText == null) return new Size();

        _formattedText.MaxTextWidth = Math.Min(3579139, availableSize.Width);
        _formattedText.MaxTextHeight = Math.Max(0.0001d, availableSize.Height);

        UpdatePen();

        return new Size(_formattedText.Width, _formattedText.Height);
    }
}