using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace HandyControl.Controls;

[TemplatePart(ElementButtonConfirm, typeof(Button))]
[TemplatePart(ElementClockPresenter, typeof(ContentPresenter))]
[TemplatePart(ElementCalendarPresenter, typeof(ContentPresenter))]
public class CalendarWithClock : TemplatedControl
{
    private const string ElementButtonConfirm = "PART_ButtonConfirm";

    private const string ElementClockPresenter = "PART_ClockPresenter";

    private const string ElementCalendarPresenter = "PART_CalendarPresenter";

    private ContentPresenter? _clockPresenter;

    private ContentPresenter? _calendarPresenter;

    private Clock? _clock;

    private Calendar? _calendar;

    private Button? _buttonConfirm;

    private bool _isLoaded;

    private bool _isHandlerSuspended;

    public event EventHandler<DateTime?>? SelectedDateTimeChanged;

    public event EventHandler<DateTime>? DisplayDateTimeChanged;

    public event Action? Confirmed;

    public static readonly StyledProperty<string> DateTimeFormatProperty =
        AvaloniaProperty.Register<CalendarWithClock, string>(nameof(DateTimeFormat), "yyyy-MM-dd HH:mm:ss");

    public string DateTimeFormat
    {
        get => GetValue(DateTimeFormatProperty);
        set => SetValue(DateTimeFormatProperty, value);
    }

    public static readonly StyledProperty<bool> ShowConfirmButtonProperty =
        AvaloniaProperty.Register<CalendarWithClock, bool>(nameof(ShowConfirmButton));

    public bool ShowConfirmButton
    {
        get => GetValue(ShowConfirmButtonProperty);
        set => SetValue(ShowConfirmButtonProperty, value);
    }

    public static readonly StyledProperty<DateTime?> SelectedDateTimeProperty =
        AvaloniaProperty.Register<CalendarWithClock, DateTime?>(nameof(SelectedDateTime),
            defaultBindingMode: BindingMode.TwoWay);

    public DateTime? SelectedDateTime
    {
        get => GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    public static readonly StyledProperty<DateTime> DisplayDateTimeProperty =
        AvaloniaProperty.Register<CalendarWithClock, DateTime>(nameof(DisplayDateTime), DateTime.MinValue,
            defaultBindingMode: BindingMode.TwoWay);

    public DateTime DisplayDateTime
    {
        get => GetValue(DisplayDateTimeProperty);
        set => SetValue(DisplayDateTimeProperty, value);
    }

    static CalendarWithClock()
    {
        SelectedDateTimeProperty.Changed.AddClassHandler<CalendarWithClock>((o, e) => o.OnSelectedDateTimeChanged(e));
        DisplayDateTimeProperty.Changed.AddClassHandler<CalendarWithClock>((o, e) => o.OnDisplayDateTimeChanged(e));
    }

    public CalendarWithClock()
    {
        InitCalendarAndClock();
        Loaded += (s, e) =>
        {
            if (_isLoaded) return;
            _isLoaded = true;
            SetCurrentValue(DisplayDateTimeProperty, SelectedDateTime ?? DateTime.Now);
        };
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_buttonConfirm != null)
        {
            _buttonConfirm.Click -= ButtonConfirm_OnClick;
        }

        base.OnApplyTemplate(e);

        _buttonConfirm = e.NameScope.Find<Button>(ElementButtonConfirm);
        _clockPresenter = e.NameScope.Find<ContentPresenter>(ElementClockPresenter);
        _calendarPresenter = e.NameScope.Find<ContentPresenter>(ElementCalendarPresenter);

        if (_clockPresenter != null) _clockPresenter.Content = _clock;
        if (_calendarPresenter != null) _calendarPresenter.Content = _calendar;

        if (_buttonConfirm != null) _buttonConfirm.Click += ButtonConfirm_OnClick;
    }

    private void OnSelectedDateTimeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        SelectedDateTimeChanged?.Invoke(this, (DateTime?) e.NewValue);
    }

    private void OnDisplayDateTimeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isHandlerSuspended) return;
        if (e.NewValue is not DateTime v) return;

        if (_clock != null) _clock.SelectedTime = v;
        if (_calendar != null)
        {
            _calendar.SelectedDate = v;
            _calendar.DisplayDate = v;
        }

        DisplayDateTimeChanged?.Invoke(this, v);
    }

    private void SetValueNoCallback(AvaloniaProperty property, object? value)
    {
        _isHandlerSuspended = true;
        try
        {
            SetCurrentValue(property, value);
        }
        finally
        {
            _isHandlerSuspended = false;
        }
    }

    private void ButtonConfirm_OnClick(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(SelectedDateTimeProperty, DisplayDateTime);
        Confirmed?.Invoke();
    }

    private void InitCalendarAndClock()
    {
        _clock = new Clock
        {
            BorderThickness = new Thickness(),
            Background = Brushes.Transparent
        };
        TitleElement.SetBackground(_clock, Brushes.Transparent);
        _clock.DisplayTimeChanged += Clock_DisplayTimeChanged;

        _calendar = new Calendar
        {
            BorderThickness = new Thickness(),
            Background = Brushes.Transparent,
            Focusable = false
        };
        TitleElement.SetBackground(_calendar, Brushes.Transparent);
        _calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
    }

    private void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateDisplayTime();
    }

    private void Clock_DisplayTimeChanged(object? sender, DateTime e) => UpdateDisplayTime();

    private void UpdateDisplayTime()
    {
        if (_calendar?.SelectedDate != null)
        {
            var date = _calendar.SelectedDate.Value;
            var time = _clock?.DisplayTime ?? DateTime.Now;

            var result = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
            SetValueNoCallback(DisplayDateTimeProperty, result);
        }
    }
}
