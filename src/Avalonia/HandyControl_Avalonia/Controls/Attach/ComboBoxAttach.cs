using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace HandyControl.Controls;

public class ComboBoxAttach
{
    public static readonly AttachedProperty<bool> IsMouseWheelEnabledProperty =
        AvaloniaProperty.RegisterAttached<ComboBoxAttach, AvaloniaObject, bool>("IsMouseWheelEnabled", true);

    public static void SetIsMouseWheelEnabled(AvaloniaObject element, bool value) =>
        element.SetValue(IsMouseWheelEnabledProperty, value);

    public static bool GetIsMouseWheelEnabled(AvaloniaObject element) =>
        element.GetValue(IsMouseWheelEnabledProperty);

    static ComboBoxAttach()
    {
        IsMouseWheelEnabledProperty.Changed.AddClassHandler<AvaloniaObject>(OnIsMouseWheelEnabledChanged);
    }

    private static void OnIsMouseWheelEnabledChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox)
        {
            return;
        }

        if ((bool)e.NewValue!)
        {
            comboBox.RemoveHandler(InputElement.PointerWheelChangedEvent, OnComboBoxPointerWheelChanged);
        }
        else
        {
            comboBox.AddHandler(InputElement.PointerWheelChangedEvent, OnComboBoxPointerWheelChanged);
        }
    }

    private static void OnComboBoxPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is ComboBox { IsDropDownOpen: false })
        {
            e.Handled = true;
        }
    }
}