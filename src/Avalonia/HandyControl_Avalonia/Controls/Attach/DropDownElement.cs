using Avalonia;

namespace HandyControl.Controls;

public class DropDownElement
{
    public static readonly AttachedProperty<bool> ConsistentWidthProperty =
        AvaloniaProperty.RegisterAttached<DropDownElement, AvaloniaObject, bool>("ConsistentWidth", inherits: true);

    public static void SetConsistentWidth(AvaloniaObject element, bool value) =>
        element.SetValue(ConsistentWidthProperty, value);

    public static bool GetConsistentWidth(AvaloniaObject element) =>
        element.GetValue(ConsistentWidthProperty);

    public static readonly AttachedProperty<bool> AutoWidthProperty =
        AvaloniaProperty.RegisterAttached<DropDownElement, AvaloniaObject, bool>("AutoWidth", inherits: true);

    public static void SetAutoWidth(AvaloniaObject element, bool value) =>
        element.SetValue(AutoWidthProperty, value);

    public static bool GetAutoWidth(AvaloniaObject element) =>
        element.GetValue(AutoWidthProperty);
}