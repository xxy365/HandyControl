using Avalonia;

namespace HandyControl.Controls;

public class SliderElement
{
    public static readonly AttachedProperty<bool> IsSelectionRangeEnabledProperty =
        AvaloniaProperty.RegisterAttached<SliderElement, AvaloniaObject, bool>("IsSelectionRangeEnabled");

    public static void SetIsSelectionRangeEnabled(AvaloniaObject element, bool value) =>
        element.SetValue(IsSelectionRangeEnabledProperty, value);

    public static bool GetIsSelectionRangeEnabled(AvaloniaObject element) =>
        element.GetValue<bool>(IsSelectionRangeEnabledProperty);

    public static readonly AttachedProperty<double> SelectionStartProperty =
        AvaloniaProperty.RegisterAttached<SliderElement, AvaloniaObject, double>("SelectionStart");

    public static void SetSelectionStart(AvaloniaObject element, double value) =>
        element.SetValue(SelectionStartProperty, value);

    public static double GetSelectionStart(AvaloniaObject element) =>
        element.GetValue<double>(SelectionStartProperty);

    public static readonly AttachedProperty<double> SelectionEndProperty =
        AvaloniaProperty.RegisterAttached<SliderElement, AvaloniaObject, double>("SelectionEnd");

    public static void SetSelectionEnd(AvaloniaObject element, double value) =>
        element.SetValue(SelectionEndProperty, value);

    public static double GetSelectionEnd(AvaloniaObject element) =>
        element.GetValue<double>(SelectionEndProperty);
}