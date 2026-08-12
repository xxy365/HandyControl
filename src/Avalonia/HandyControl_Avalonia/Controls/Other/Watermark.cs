using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace HandyControl.Controls;

[TemplatePart(ElementRoot, typeof(Border))]
public class Watermark : ContentControl
{
    private const string ElementRoot = "PART_Root";

    private Border? _borderRoot;

    private IBrush? _brush;

    public static readonly StyledProperty<double> AngleProperty =
        AvaloniaProperty.Register<Watermark, double>(nameof(Angle));

    public double Angle
    {
        get => GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    public static readonly StyledProperty<object?> MarkProperty =
        AvaloniaProperty.Register<Watermark, object?>(nameof(Mark));

    public object? Mark
    {
        get => GetValue(MarkProperty);
        set => SetValue(MarkProperty, value);
    }

    public static readonly StyledProperty<double> MarkWidthProperty =
        AvaloniaProperty.Register<Watermark, double>(nameof(MarkWidth));

    public double MarkWidth
    {
        get => GetValue(MarkWidthProperty);
        set => SetValue(MarkWidthProperty, value);
    }

    public static readonly StyledProperty<double> MarkHeightProperty =
        AvaloniaProperty.Register<Watermark, double>(nameof(MarkHeight));

    public double MarkHeight
    {
        get => GetValue(MarkHeightProperty);
        set => SetValue(MarkHeightProperty, value);
    }

    public static readonly StyledProperty<IBrush?> MarkBrushProperty =
        AvaloniaProperty.Register<Watermark, IBrush?>(nameof(MarkBrush));

    public IBrush? MarkBrush
    {
        get => GetValue(MarkBrushProperty);
        set => SetValue(MarkBrushProperty, value);
    }

    public static readonly StyledProperty<bool> AutoSizeEnabledProperty =
        AvaloniaProperty.Register<Watermark, bool>(nameof(AutoSizeEnabled), true);

    public bool AutoSizeEnabled
    {
        get => GetValue(AutoSizeEnabledProperty);
        set => SetValue(AutoSizeEnabledProperty, value);
    }

    public static readonly StyledProperty<Thickness> MarkMarginProperty =
        AvaloniaProperty.Register<Watermark, Thickness>(nameof(MarkMargin));

    public Thickness MarkMargin
    {
        get => GetValue(MarkMarginProperty);
        set => SetValue(MarkMarginProperty, value);
    }

    static Watermark()
    {
        AngleProperty.Changed.AddClassHandler<Watermark>((o, e) => o.EnsureBrush());
        MarkProperty.Changed.AddClassHandler<Watermark>((o, e) => o.EnsureBrush());
        MarkWidthProperty.Changed.AddClassHandler<Watermark>((o, e) => o.EnsureBrush());
        MarkHeightProperty.Changed.AddClassHandler<Watermark>((o, e) => o.EnsureBrush());
        MarkBrushProperty.Changed.AddClassHandler<Watermark>((o, e) => o.EnsureBrush());
        AutoSizeEnabledProperty.Changed.AddClassHandler<Watermark>((o, e) => o.EnsureBrush());
        MarkMarginProperty.Changed.AddClassHandler<Watermark>((o, e) => o.EnsureBrush());
        FontSizeProperty.Changed.AddClassHandler<Watermark>((o, e) => o.EnsureBrush());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _borderRoot = e.NameScope.Find<Border>(ElementRoot);
        EnsureBrush();
    }

    private void EnsureBrush()
    {
        var presenter = new ContentPresenter();

        if (Mark is Geometry geometry)
        {
            presenter.Content = new Path
            {
                Width = MarkWidth,
                Height = MarkHeight,
                Fill = MarkBrush,
                Stretch = Stretch.Uniform,
                Data = geometry
            };
        }
        else if (Mark is string str)
        {
            presenter.Content = new TextBlock
            {
                Text = str,
                FontSize = FontSize,
                Foreground = MarkBrush
            };
        }
        else
        {
            presenter.Content = Mark;
        }

        Size markSize;
        if (AutoSizeEnabled)
        {
            presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            markSize = presenter.DesiredSize;
        }
        else
        {
            markSize = new Size(MarkWidth, MarkHeight);
        }

        var border = new Border
        {
            Background = Brushes.Transparent,
            Padding = MarkMargin,
            Child = presenter
        };
        border.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var borderSize = border.DesiredSize;
        border.Arrange(new Rect(borderSize));

        _brush = new DrawingBrush
        {
            TileMode = TileMode.Tile,
            Stretch = Stretch.Uniform,
            Transform = new RotateTransform(Angle),
            DestinationRect = new RelativeRect(markSize, RelativeUnit.Absolute),
            Drawing = new GeometryDrawing
            {
                Brush = new VisualBrush(border),
                Geometry = new RectangleGeometry(new Rect(markSize))
            }
        };

        if (_borderRoot != null)
        {
            _borderRoot.Background = _brush;
        }
    }
}
