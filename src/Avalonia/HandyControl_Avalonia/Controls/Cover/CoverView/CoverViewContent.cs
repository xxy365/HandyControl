using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using HandyControl.Tools;

namespace HandyControl.Controls;

public class CoverViewContent : ContentControl
{
    private const string ElementTriangle = "PART_Triangle";

    private const string ElementContent = "PART_Content";

    private Control? _triangle;

    private Control? _content;

    internal bool WaitForUpdate { get; set; }

    private int _index;

    private int _groups;

    private double _itemWidth;

    internal bool CanSwitch { get; set; } = true;

    private bool _isOpen;

    internal bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value) return;
            _isOpen = value;
            OpenSwitch(value);
        }
    }

    internal double ManualHeight { get; set; }

    public static readonly StyledProperty<double> ContentHeightProperty =
        AvaloniaProperty.Register<CoverViewContent, double>(nameof(ContentHeight), 300.0);

    public double ContentHeight
    {
        get => GetValue(ContentHeightProperty);
        set => SetValue(ContentHeightProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _triangle = e.NameScope.Find<Control>(ElementTriangle);
        _content = e.NameScope.Find<Control>(ElementContent);

        if (WaitForUpdate)
        {
            SetTriangleMargin(new Thickness((_index % _groups + .5) * _itemWidth - (_triangle?.Width ?? 30) / 2, 0, 0, 0));
            OpenSwitch(_isOpen);
            WaitForUpdate = false;
        }
    }

    internal void UpdatePosition(int index, int groups, double itemWidth)
    {
        if (_triangle == null || _content == null)
        {
            _index = index;
            _groups = groups;
            _itemWidth = itemWidth;
            WaitForUpdate = true;
            return;
        }

        SetTriangleMargin(new Thickness((index % groups + .5) * itemWidth - _triangle.Width / 2, 0, 0, 0));

        if (IsOpen)
        {
            var height = ManualHeight > 0 && !MathHelper.AreClose(ManualHeight, ContentHeight)
                ? ManualHeight
                : ContentHeight;
            _content.Height = height;
        }
    }

    private void OpenSwitch(bool isOpen)
    {
        if (_content == null) return;

        _content.Height = isOpen
            ? ManualHeight > 0 ? ManualHeight : ContentHeight
            : 0;
        IsVisible = isOpen;

        CanSwitch = true;
    }

    private void SetTriangleMargin(Thickness margin)
    {
        if (_triangle != null)
        {
            _triangle.Margin = margin;
        }
    }
}