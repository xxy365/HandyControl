using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using HandyControl.Data;

namespace HandyControl.Controls;

public class PreviewSlider : Slider
{
    private const string TrackKey = "PART_Track";

    private const string ThumbKey = "PART_Thumb";

    private ContentControl? _previewContent;

    private Thumb? _thumb;

    private TranslateTransform? _transform;

    private Track? _track;

    private Canvas? _adornerHost;

    private bool _isAdornerAttached;

    public static readonly StyledProperty<object?> PreviewContentProperty = AvaloniaProperty.Register<PreviewSlider, object?>(
        nameof(PreviewContent));

    public object? PreviewContent
    {
        get => GetValue(PreviewContentProperty);
        set => SetValue(PreviewContentProperty, value);
    }

    public static readonly StyledProperty<double> PreviewContentOffsetProperty = AvaloniaProperty.Register<PreviewSlider, double>(
        nameof(PreviewContentOffset), 9.0);

    public double PreviewContentOffset
    {
        get => GetValue(PreviewContentOffsetProperty);
        set => SetValue(PreviewContentOffsetProperty, value);
    }

    public static readonly AttachedProperty<double> PreviewPositionProperty =
        AvaloniaProperty.RegisterAttached<PreviewSlider, Control, double>("PreviewPosition", inherits: true);

    public static void SetPreviewPosition(AvaloniaObject element, double value)
        => element.SetValue(PreviewPositionProperty, value);

    public static double GetPreviewPosition(AvaloniaObject element)
        => element.GetValue(PreviewPositionProperty);

    public double PreviewPosition
    {
        get => _previewContent == null ? default : GetPreviewPosition(_previewContent);
        set
        {
            if (_previewContent != null)
            {
                SetPreviewPosition(_previewContent, value);
            }
        }
    }

    public static readonly RoutedEvent<FunctionEventArgs<double>> PreviewPositionChangedEvent =
        RoutedEvent.Register<PreviewSlider, FunctionEventArgs<double>>(nameof(PreviewPositionChanged),
            RoutingStrategies.Bubble);

    public event EventHandler<FunctionEventArgs<double>>? PreviewPositionChanged
    {
        add => AddHandler(PreviewPositionChangedEvent, value);
        remove => RemoveHandler(PreviewPositionChangedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _track = e.NameScope.Find<Track>(TrackKey);
        _thumb = e.NameScope.Find<Thumb>(ThumbKey);

        _previewContent = new ContentControl
        {
            DataContext = this,
            Content = PreviewContent
        };

        _transform = new TranslateTransform();

        _previewContent.HorizontalAlignment = HorizontalAlignment.Left;
        _previewContent.VerticalAlignment = VerticalAlignment.Top;
        _previewContent.RenderTransform = _transform;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PreviewContentProperty && _previewContent != null)
        {
            _previewContent.Content = change.NewValue;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_previewContent == null || _track == null || _thumb == null || _transform == null) return;

        var p = e.GetPosition(_track);
        var maximum = Maximum;
        var minimum = Minimum;

        var isDragging = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;

        if (Orientation == Orientation.Horizontal)
        {
            var pos = !IsDirectionReversed
                ? (e.GetPosition(this).X - _thumb.Bounds.Width * 0.5) / _track.Bounds.Width * (maximum - minimum) + minimum
                : (1 - (e.GetPosition(this).X - _thumb.Bounds.Width * 0.5) / _track.Bounds.Width) * (maximum - minimum) + minimum;
            if (pos > maximum || pos < 0)
            {
                if (isDragging)
                {
                    PreviewPosition = Value;
                }
                return;
            }

            _transform.X = p.X - _previewContent.Bounds.Width * 0.5;
            var translated = _thumb.TranslatePoint(new Point(), _track);
            _transform.Y = (translated?.Y ?? 0) - _previewContent.Bounds.Height - PreviewContentOffset;

            PreviewPosition = isDragging ? Value : pos;
        }
        else
        {
            var pos = !IsDirectionReversed
                ? (1 - (e.GetPosition(this).Y - _thumb.Bounds.Height * 0.5) / _track.Bounds.Height) * (maximum - minimum) + minimum
                : (e.GetPosition(this).Y - _thumb.Bounds.Height * 0.5) / _track.Bounds.Height * (maximum - minimum) + minimum;
            if (pos > maximum || pos < 0)
            {
                if (isDragging)
                {
                    PreviewPosition = Value;
                }
                return;
            }

            var translatedX = _thumb.TranslatePoint(new Point(), _track);
            _transform.X = (translatedX?.X ?? 0) - _previewContent.Bounds.Width - PreviewContentOffset;
            _transform.Y = p.Y - _previewContent.Bounds.Height * 0.5;

            PreviewPosition = isDragging ? Value : pos;
        }

        RaiseEvent(new FunctionEventArgs<double>(PreviewPositionChangedEvent, this)
        {
            Info = PreviewPosition
        });
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (_previewContent == null || _adornerHost != null) return;

        var host = new Canvas
        {
            IsHitTestVisible = false
        };
        host.Children.Add(_previewContent);
        _adornerHost = host;
        _isAdornerAttached = true;
        AdornerLayer.SetAdorner(this, host);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (_adornerHost == null || !_isAdornerAttached) return;

        AdornerLayer.SetAdorner(this, null);
        _adornerHost.Children.Clear();
        _adornerHost = null;
        _isAdornerAttached = false;
    }

    internal TranslateTransform? _Translate => _transform;
}