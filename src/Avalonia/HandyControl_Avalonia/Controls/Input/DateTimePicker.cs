using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using HandyControl.Data;

namespace HandyControl.Controls;

[TemplatePart(ElementRoot, typeof(Grid))]
[TemplatePart(ElementTextBox, typeof(WatermarkTextBox))]
[TemplatePart(ElementButton, typeof(Button))]
[TemplatePart(ElementPopup, typeof(Popup))]
[PseudoClasses(":dropdownopen")]
public class DateTimePicker : TemplatedControl
{
    private const string ElementRoot = "PART_Root";
    private const string ElementTextBox = "PART_TextBox";
    private const string ElementButton = "PART_Button";
    private const string ElementPopup = "PART_Popup";

    private CalendarWithClock? _calendarWithClock;
    private string? _defaultText;
    private Button? _dropDownButton;
    private Popup? _popup;
    private WatermarkTextBox? _textBox;
    private DateTime? _originalSelectedDateTime;
    private bool _disablePopupReopen;
    private bool _isHandlerSuspendedText;

    public event EventHandler<DateTime?>? SelectedDateTimeChanged;
    public event EventHandler? PickerClosed;
    public event EventHandler? PickerOpened;

    public static readonly StyledProperty<string> DateTimeFormatProperty =
        AvaloniaProperty.Register<DateTimePicker, string>(nameof(DateTimeFormat), "yyyy-MM-dd HH:mm:ss");

    public string DateTimeFormat
    {
        get => GetValue(DateTimeFormatProperty);
        set => SetValue(DateTimeFormatProperty, value);
    }

    public static readonly StyledProperty<DateTime> DisplayDateTimeProperty =
        AvaloniaProperty.Register<DateTimePicker, DateTime>(nameof(DisplayDateTime), DateTime.Now,
            defaultBindingMode: BindingMode.TwoWay);

    public DateTime DisplayDateTime
    {
        get => GetValue(DisplayDateTimeProperty);
        set => SetValue(DisplayDateTimeProperty, value);
    }

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<DateTimePicker, bool>(nameof(IsDropDownOpen),
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly StyledProperty<DateTime?> SelectedDateTimeProperty =
        AvaloniaProperty.Register<DateTimePicker, DateTime?>(nameof(SelectedDateTime),
            defaultBindingMode: BindingMode.TwoWay);

    public DateTime? SelectedDateTime
    {
        get => GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<DateTimePicker, string>(nameof(Text), string.Empty);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<string?> WatermarkProperty =
        AvaloniaProperty.Register<DateTimePicker, string?>(nameof(Watermark));

    public string? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        TextBox.SelectionBrushProperty.AddOwner<DateTimePicker>();

    public IBrush? SelectionBrush
    {
        get => GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public static readonly StyledProperty<IBrush?> SelectionForegroundBrushProperty =
        TextBox.SelectionForegroundBrushProperty.AddOwner<DateTimePicker>();

    public IBrush? SelectionForegroundBrush
    {
        get => GetValue(SelectionForegroundBrushProperty);
        set => SetValue(SelectionForegroundBrushProperty, value);
    }

    public static readonly StyledProperty<IBrush?> CaretBrushProperty =
        TextBox.CaretBrushProperty.AddOwner<DateTimePicker>();

    public IBrush? CaretBrush
    {
        get => GetValue(CaretBrushProperty);
        set => SetValue(CaretBrushProperty, value);
    }

    public static readonly StyledProperty<Avalonia.Layout.HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<DateTimePicker, Avalonia.Layout.HorizontalAlignment>(
            nameof(HorizontalContentAlignment), Avalonia.Layout.HorizontalAlignment.Left);

    public Avalonia.Layout.HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public static readonly StyledProperty<Avalonia.Layout.VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<DateTimePicker, Avalonia.Layout.VerticalAlignment>(
            nameof(VerticalContentAlignment), Avalonia.Layout.VerticalAlignment.Center);

    public Avalonia.Layout.VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    static DateTimePicker()
    {
        IsDropDownOpenProperty.Changed.AddClassHandler<DateTimePicker>((o, e) => o.OnIsDropDownOpenChanged(e));
        SelectedDateTimeProperty.Changed.AddClassHandler<DateTimePicker>((o, e) => o.OnSelectedDateTimeChanged(e));
        TextProperty.Changed.AddClassHandler<DateTimePicker>((o, e) => o.OnTextPropertyChanged(e));
        DisplayDateTimeProperty.Changed.AddClassHandler<DateTimePicker>((o, e) => o.OnDisplayDateTimeChanged(e));
    }

    public DateTimePicker()
    {
        InitCalendarWithClock();
        UpdatePseudoClasses();
    }

    private void OnIsDropDownOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var newValue = (bool) (e.NewValue ?? false);
        if (!IsEnabled) newValue = false;

        if (_popup != null && _popup.IsOpen != newValue)
        {
            _popup.IsOpen = newValue;
            if (newValue)
            {
                _originalSelectedDateTime = SelectedDateTime;
                _calendarWithClock?.Focus();
            }
        }
        UpdatePseudoClasses();
    }

    private void OnSelectedDateTimeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (SelectedDateTime.HasValue)
        {
            SetTextInternal(DateTimeToString(SelectedDateTime.Value));
        }
        else
        {
            SetTextInternal(string.Empty);
        }

        SelectedDateTimeChanged?.Invoke(this, SelectedDateTime);
    }

    private void OnDisplayDateTimeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_calendarWithClock != null && e.NewValue is DateTime dt)
        {
            _calendarWithClock.DisplayDateTime = dt;
        }
    }

    private void OnTextPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isHandlerSuspendedText) return;
        if (e.NewValue is string newValue)
        {
            if (_textBox != null)
            {
                _textBox.Text = newValue;
            }
            else
            {
                _defaultText = newValue;
            }

            SetSelectedDateTime();
        }
        else
        {
            SetValueNoCallback(SelectedDateTimeProperty, null);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_popup != null)
        {
            _popup.PointerPressed -= Popup_PointerPressed;
            _popup.Opened -= PopupOpened;
            _popup.Closed -= PopupClosed;
            _popup.Child = null;
        }
        if (_dropDownButton != null)
        {
            _dropDownButton.Click -= DropDownButton_Click;
        }
        if (_textBox != null)
        {
            _textBox.KeyDown -= TextBox_KeyDown;
            _textBox.TextChanged -= TextBox_TextChanged;
            _textBox.LostFocus -= TextBox_LostFocus;
        }

        base.OnApplyTemplate(e);

        _popup = e.NameScope.Find<Popup>(ElementPopup);
        _dropDownButton = e.NameScope.Find<Button>(ElementButton);
        _textBox = e.NameScope.Find<WatermarkTextBox>(ElementTextBox);

        if (_popup != null)
        {
            _popup.PointerPressed += Popup_PointerPressed;
            _popup.Opened += PopupOpened;
            _popup.Closed += PopupClosed;
            _popup.Child = _calendarWithClock;
        }
        if (_dropDownButton != null)
        {
            _dropDownButton.Click += DropDownButton_Click;
        }
        if (_textBox != null)
        {
            _textBox.KeyDown += TextBox_KeyDown;
            _textBox.TextChanged += TextBox_TextChanged;
            _textBox.LostFocus += TextBox_LostFocus;

            var selectedDateTime = SelectedDateTime;
            if (selectedDateTime == null)
            {
                if (!string.IsNullOrEmpty(_defaultText))
                {
                    _textBox.Text = _defaultText;
                    SetSelectedDateTime();
                }
            }
            else
            {
                _textBox.Text = DateTimeToString(selectedDateTime.Value);
            }
        }

        if (SelectedDateTime is null)
        {
            _originalSelectedDateTime ??= DateTime.Now;
            SetCurrentValue(DisplayDateTimeProperty, _originalSelectedDateTime.Value);
        }
        else
        {
            SetCurrentValue(DisplayDateTimeProperty, SelectedDateTime.Value);
        }
    }

    private void SetTextInternal(string value) => SetCurrentValue(TextProperty, value);

    private void SetValueNoCallback(AvaloniaProperty property, object? value)
    {
        _isHandlerSuspendedText = true;
        try { SetCurrentValue(property, value); }
        finally { _isHandlerSuspendedText = false; }
    }

    private void InitCalendarWithClock()
    {
        _calendarWithClock = new CalendarWithClock
        {
            ShowConfirmButton = true
        };
        _calendarWithClock.SelectedDateTimeChanged += CalendarWithClock_SelectedDateTimeChanged;
        _calendarWithClock.Confirmed += CalendarWithClock_Confirmed;
    }

    private void CalendarWithClock_Confirmed() => TogglePopup();

    private void CalendarWithClock_SelectedDateTimeChanged(object? sender, DateTime? e) => SelectedDateTime = e;

    private void TextBox_LostFocus(object? sender, RoutedEventArgs e) => SetSelectedDateTime();

    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_textBox == null) return;
        SetValueNoCallback(TextProperty, _textBox.Text ?? string.Empty);
    }

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down && (e.KeyModifiers & KeyModifiers.Alt) != 0)
        {
            TogglePopup();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            SetSelectedDateTime();
            e.Handled = true;
        }
    }

    private void Popup_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_dropDownButton != null)
        {
            var position = e.GetPosition(_dropDownButton);
            var hit = _dropDownButton.InputHitTest(position);
            if (hit != null)
            {
                _disablePopupReopen = true;
            }
        }
    }

    private void PopupOpened(object? sender, EventArgs e)
    {
        if (!IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, true);

        _calendarWithClock?.Focus();

        PickerOpened?.Invoke(this, EventArgs.Empty);
    }

    private void PopupClosed(object? sender, EventArgs e)
    {
        if (IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, false);

        if (_calendarWithClock?.IsKeyboardFocusWithin == true)
        {
            Focus();
        }

        PickerClosed?.Invoke(this, EventArgs.Empty);
    }

    private void DropDownButton_Click(object? sender, RoutedEventArgs e) => TogglePopup();

    private void TogglePopup()
    {
        if (IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
        else
        {
            if (_disablePopupReopen)
            {
                _disablePopupReopen = false;
            }
            else
            {
                SetSelectedDateTime();
                SetCurrentValue(IsDropDownOpenProperty, true);
            }
        }
    }

    private void SafeSetText(string s)
    {
        if (string.Compare(Text, s, StringComparison.Ordinal) != 0)
        {
            SetCurrentValue(TextProperty, s);
        }
    }

    private DateTime? ParseText(string text)
    {
        if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var d))
            return d;
        return null;
    }

    private DateTime? SetTextBoxValue(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            SafeSetText(s);
            return SelectedDateTime;
        }

        var d = ParseText(s);

        if (d != null)
        {
            SafeSetText(DateTimeToString((DateTime) d));
            return d;
        }

        if (SelectedDateTime != null)
        {
            SafeSetText(DateTimeToString(SelectedDateTime.Value));
            return SelectedDateTime;
        }

        SafeSetText(DateTimeToString(DisplayDateTime));
        return DisplayDateTime;
    }

    private void SetSelectedDateTime()
    {
        if (_textBox != null)
        {
            if (!string.IsNullOrEmpty(_textBox.Text))
            {
                var s = _textBox.Text;

                if (SelectedDateTime != null)
                {
                    if (SelectedDateTime != DisplayDateTime)
                    {
                        SetCurrentValue(DisplayDateTimeProperty, SelectedDateTime);
                    }

                    var selectedTime = DateTimeToString(SelectedDateTime.Value);

                    if (string.Compare(selectedTime, s, StringComparison.Ordinal) == 0)
                    {
                        return;
                    }
                }

                var d = SetTextBoxValue(s);
                if (!Equals(SelectedDateTime, d))
                {
                    SetCurrentValue(SelectedDateTimeProperty, d);
                    SetCurrentValue(DisplayDateTimeProperty, d);
                }
            }
            else
            {
                if (SelectedDateTime.HasValue)
                {
                    SetCurrentValue(SelectedDateTimeProperty, null);
                }
                else
                {
                    SetTextInternal(string.Empty);
                }
            }
        }
        else
        {
            var d = SetTextBoxValue(_defaultText ?? string.Empty);
            if (!Equals(SelectedDateTime, d))
            {
                SetCurrentValue(SelectedDateTimeProperty, d);
            }
        }
    }

    private string DateTimeToString(DateTime d) => d.ToString(DateTimeFormat, CultureInfo.CurrentCulture);

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":dropdownopen", IsDropDownOpen);
    }

    public override string ToString() => SelectedDateTime?.ToString(DateTimeFormat) ?? string.Empty;

    public void Clear()
    {
        SetCurrentValue(SelectedDateTimeProperty, null);
        SetCurrentValue(TextProperty, string.Empty);
    }
}
