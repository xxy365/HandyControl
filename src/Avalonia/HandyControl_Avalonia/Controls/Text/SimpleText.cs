using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using HandyControl.Tools.Helper;

namespace HandyControl.Controls;

public class SimpleText : Control
{
    private FormattedText? _formattedText;

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<SimpleText, string>(nameof(Text), string.Empty);

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<SimpleText, TextAlignment>(nameof(TextAlignment));

    public static readonly StyledProperty<TextTrimming> TextTrimmingProperty =
        AvaloniaProperty.Register<SimpleText, TextTrimming>(nameof(TextTrimming));

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<SimpleText, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<SimpleText, IBrush?>(nameof(Foreground), Brushes.Black);

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        AvaloniaProperty.Register<SimpleText, FontFamily>(nameof(FontFamily), FontFamily.Default);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<SimpleText, double>(nameof(FontSize), 12.0);

    public static readonly StyledProperty<FontStretch> FontStretchProperty =
        AvaloniaProperty.Register<SimpleText, FontStretch>(nameof(FontStretch), FontStretch.Normal);

    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        AvaloniaProperty.Register<SimpleText, FontStyle>(nameof(FontStyle), FontStyle.Normal);

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<SimpleText, FontWeight>(nameof(FontWeight), FontWeight.Normal);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty || change.Property == TextWrappingProperty || change.Property == TextTrimmingProperty)
        {
            _formattedText = null;
            InvalidateMeasure();
            InvalidateVisual();
        }
        else
        {
            UpdateFormattedText();
            InvalidateMeasure();
            InvalidateVisual();
        }
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

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
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
        EnsureFormattedText();
        if (_formattedText != null)
        {
            context.DrawText(_formattedText, new Point());
        }
    }

    private void EnsureFormattedText()
    {
        if (_formattedText != null || Text == null) return;

        _formattedText = TextHelper.CreateFormattedText(Text, FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize);

        UpdateFormattedText();
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
        if (Foreground != null)
        {
            _formattedText.SetForegroundBrush(Foreground);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureFormattedText();
        if (_formattedText == null) return new Size();

        _formattedText.MaxTextWidth = Math.Min(3579139, availableSize.Width);
        _formattedText.MaxTextHeight = Math.Max(0.0001d, availableSize.Height);

        return new Size(_formattedText.Width, _formattedText.Height);
    }
}