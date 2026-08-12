using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;

namespace HandyControl.Controls;

[TemplatePart(Name = ElementPasswordBox, Type = typeof(TextBox))]
public class PasswordBox : TemplatedControl
{
    private const string ElementPasswordBox = "PART_PasswordBox";

    private TextBox? _passwordBox;

    public static readonly StyledProperty<char> PasswordCharProperty =
        AvaloniaProperty.Register<PasswordBox, char>(nameof(PasswordChar), '\u25CF');

    public static readonly StyledProperty<bool> ShowEyeButtonProperty =
        AvaloniaProperty.Register<PasswordBox, bool>(nameof(ShowEyeButton));

    public static readonly StyledProperty<bool> ShowPasswordProperty =
        AvaloniaProperty.Register<PasswordBox, bool>(nameof(ShowPassword), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsSafeEnabledProperty =
        AvaloniaProperty.Register<PasswordBox, bool>(nameof(IsSafeEnabled), true);

    public static readonly StyledProperty<string> UnsafePasswordProperty =
        AvaloniaProperty.Register<PasswordBox, string>(nameof(UnsafePassword), string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> PasswordProperty =
        AvaloniaProperty.Register<PasswordBox, string>(nameof(Password), string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<int> MaxLengthProperty =
        TextBox.MaxLengthProperty.AddOwner<PasswordBox>();

    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        TextBox.SelectionBrushProperty.AddOwner<PasswordBox>();

    public static readonly StyledProperty<IBrush?> SelectionTextBrushProperty =
        TextBox.SelectionForegroundBrushProperty.AddOwner<PasswordBox>();

    public static readonly StyledProperty<IBrush?> CaretBrushProperty =
        TextBox.CaretBrushProperty.AddOwner<PasswordBox>();

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<PasswordBox, VerticalAlignment>(nameof(VerticalContentAlignment));

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<PasswordBox, HorizontalAlignment>(nameof(HorizontalContentAlignment));

    static PasswordBox()
    {
        IsSafeEnabledProperty.Changed.AddClassHandler<PasswordBox>((o, e) => o.OnIsSafeEnabledChanged(e));
        UnsafePasswordProperty.Changed.AddClassHandler<PasswordBox>((o, e) => o.OnUnsafePasswordChanged(e));
        PasswordProperty.Changed.AddClassHandler<PasswordBox>((o, e) => o.OnPasswordChanged(e));
    }

    public char PasswordChar
    {
        get => GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    public bool ShowEyeButton
    {
        get => GetValue(ShowEyeButtonProperty);
        set => SetValue(ShowEyeButtonProperty, value);
    }

    public bool ShowPassword
    {
        get => GetValue(ShowPasswordProperty);
        set => SetValue(ShowPasswordProperty, value);
    }

    public bool IsSafeEnabled
    {
        get => GetValue(IsSafeEnabledProperty);
        set => SetValue(IsSafeEnabledProperty, value);
    }

    public string UnsafePassword
    {
        get => GetValue(UnsafePasswordProperty);
        set => SetValue(UnsafePasswordProperty, value);
    }

    public string Password
    {
        get => GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public int MaxLength
    {
        get => GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public IBrush? SelectionBrush
    {
        get => GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public IBrush? SelectionTextBrush
    {
        get => GetValue(SelectionTextBrushProperty);
        set => SetValue(SelectionTextBrushProperty, value);
    }

    public IBrush? CaretBrush
    {
        get => GetValue(CaretBrushProperty);
        set => SetValue(CaretBrushProperty, value);
    }

    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public TextBox? ActualPasswordBox => _passwordBox;

    public void Paste()
    {
        _passwordBox?.Paste();
    }

    public void SelectAll()
    {
        _passwordBox?.SelectAll();
    }

    public void Clear()
    {
        SetCurrentValue(PasswordProperty, string.Empty);
        _passwordBox?.Clear();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _passwordBox = e.NameScope.Find<TextBox>(ElementPasswordBox);
    }

    private void OnIsSafeEnabledChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if ((bool) e.NewValue!)
        {
            SetCurrentValue(UnsafePasswordProperty, string.Empty);
        }
        else
        {
            SetCurrentValue(UnsafePasswordProperty, Password);
        }
    }

    private void OnUnsafePasswordChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!IsSafeEnabled)
        {
            var value = e.NewValue as string ?? string.Empty;
            if (!string.Equals(Password, value, StringComparison.Ordinal))
            {
                SetCurrentValue(PasswordProperty, value);
            }
        }
    }

    private void OnPasswordChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!IsSafeEnabled)
        {
            var value = e.NewValue as string ?? string.Empty;
            if (!string.Equals(UnsafePassword, value, StringComparison.Ordinal))
            {
                SetCurrentValue(UnsafePasswordProperty, value);
            }
        }
    }
}
