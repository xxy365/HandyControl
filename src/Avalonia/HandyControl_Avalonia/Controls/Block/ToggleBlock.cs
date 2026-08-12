using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace HandyControl.Controls;

/// <summary>
///     切换块
/// </summary>
public class ToggleBlock : ContentControl
{
    private ContentPresenter? _contentPresenterChecked;
    private ContentPresenter? _contentPresenterUnChecked;
    private ContentPresenter? _contentPresenterIndeterminate;

    static ToggleBlock()
    {
        IsCheckedProperty.Changed.AddClassHandler<ToggleBlock>(OnIsCheckedChanged);
        CheckedContentProperty.Changed.AddClassHandler<ToggleBlock>(OnIsCheckedChanged);
        UnCheckedContentProperty.Changed.AddClassHandler<ToggleBlock>(OnIsCheckedChanged);
        IndeterminateContentProperty.Changed.AddClassHandler<ToggleBlock>(OnIsCheckedChanged);
    }

    public static readonly StyledProperty<bool?> IsCheckedProperty =
        AvaloniaProperty.Register<ToggleBlock, bool?>(nameof(IsChecked),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private static void OnIsCheckedChanged(ToggleBlock block, AvaloniaPropertyChangedEventArgs e) =>
        block.UpdateCheckedContent();

    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public static readonly StyledProperty<object?> CheckedContentProperty =
        AvaloniaProperty.Register<ToggleBlock, object?>(nameof(CheckedContent));

    public object? CheckedContent
    {
        get => GetValue(CheckedContentProperty);
        set => SetValue(CheckedContentProperty, value);
    }

    public static readonly StyledProperty<object?> UnCheckedContentProperty =
        AvaloniaProperty.Register<ToggleBlock, object?>(nameof(UnCheckedContent));

    public object? UnCheckedContent
    {
        get => GetValue(UnCheckedContentProperty);
        set => SetValue(UnCheckedContentProperty, value);
    }

    public static readonly StyledProperty<object?> IndeterminateContentProperty =
        AvaloniaProperty.Register<ToggleBlock, object?>(nameof(IndeterminateContent));

    public object? IndeterminateContent
    {
        get => GetValue(IndeterminateContentProperty);
        set => SetValue(IndeterminateContentProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _contentPresenterChecked = e.NameScope.Find<ContentPresenter>("ContentPresenterChecked");
        _contentPresenterUnChecked = e.NameScope.Find<ContentPresenter>("ContentPresenterUnChecked");
        _contentPresenterIndeterminate = e.NameScope.Find<ContentPresenter>("ContentPresenterIndeterminate");

        UpdateCheckedContent();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            SetCurrentValue(IsCheckedProperty, IsChecked == true ? false : true);
            e.Handled = true;
        }
    }

    private void UpdateCheckedContent()
    {
        var isChecked = IsChecked;

        if (_contentPresenterChecked != null)
        {
            _contentPresenterChecked.IsVisible = isChecked == true;
        }

        if (_contentPresenterUnChecked != null)
        {
            _contentPresenterUnChecked.IsVisible = isChecked == false;
        }

        if (_contentPresenterIndeterminate != null)
        {
            _contentPresenterIndeterminate.IsVisible = isChecked == null;
        }
    }
}
