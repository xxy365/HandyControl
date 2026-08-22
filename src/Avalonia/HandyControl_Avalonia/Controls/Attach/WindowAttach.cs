using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace HandyControl.Controls;

public class WindowAttach
{
    public static readonly AttachedProperty<bool> IsDragElementProperty =
        AvaloniaProperty.RegisterAttached<WindowAttach, AvaloniaObject, bool>("IsDragElement");

    public static void SetIsDragElement(AvaloniaObject element, bool value) =>
        element.SetValue(IsDragElementProperty, value);

    public static bool GetIsDragElement(AvaloniaObject element) =>
        element.GetValue(IsDragElementProperty);

    private static void OnIsDragElementChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is InputElement ctl)
        {
            if ((bool)e.NewValue!)
            {
                ctl.PointerPressed += DragElement_PointerPressed;
            }
            else
            {
                ctl.PointerPressed -= DragElement_PointerPressed;
            }
        }
    }

    private static void DragElement_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is InputElement obj && e.GetCurrentPoint(obj).Properties.IsLeftButtonPressed)
        {
            (TopLevel.GetTopLevel(obj) as Window)?.BeginMoveDrag(e);
        }
    }

    public static readonly AttachedProperty<bool> IgnoreAltF4Property =
        AvaloniaProperty.RegisterAttached<WindowAttach, AvaloniaObject, bool>("IgnoreAltF4");

    private static void OnIgnoreAltF4Changed(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is Window window)
        {
            if ((bool)e.NewValue!)
            {
                window.AddHandler(InputElement.KeyDownEvent, Window_PreviewKeyDown,
                    RoutingStrategies.Tunnel);
            }
            else
            {
                window.RemoveHandler(InputElement.KeyDownEvent, Window_PreviewKeyDown);
            }
        }
    }

    private static void Window_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Alt && e.Key == Key.F4)
        {
            e.Handled = true;
        }
    }

    public static void SetIgnoreAltF4(AvaloniaObject element, bool value) =>
        element.SetValue(IgnoreAltF4Property, value);

    public static bool GetIgnoreAltF4(AvaloniaObject element) =>
        element.GetValue(IgnoreAltF4Property);

    public static readonly AttachedProperty<bool> ShowInTaskManagerProperty =
        AvaloniaProperty.RegisterAttached<WindowAttach, AvaloniaObject, bool>("ShowInTaskManager", true);

    private static void OnShowInTaskManagerChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is Window window)
        {
            window.SetCurrentValue(Window.ShowInTaskbarProperty, (bool)e.NewValue!);
        }
    }

    public static void SetShowInTaskManager(AvaloniaObject element, bool value) =>
        element.SetValue(ShowInTaskManagerProperty, value);

    public static bool GetShowInTaskManager(AvaloniaObject element) =>
        element.GetValue(ShowInTaskManagerProperty);

    public static readonly AttachedProperty<bool> HideWhenClosingProperty =
        AvaloniaProperty.RegisterAttached<WindowAttach, AvaloniaObject, bool>("HideWhenClosing");

    private static void OnHideWhenClosingChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is Window window)
        {
            if ((bool)e.NewValue!)
            {
                window.Closing += Window_Closing;
            }
            else
            {
                window.Closing -= Window_Closing;
            }
        }
    }

    private static void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (sender is Window window)
        {
            window.Hide();
            e.Cancel = true;
        }
    }

    public static void SetHideWhenClosing(AvaloniaObject element, bool value) =>
        element.SetValue(HideWhenClosingProperty, value);

    public static bool GetHideWhenClosing(AvaloniaObject element) =>
        element.GetValue(HideWhenClosingProperty);

    static WindowAttach()
    {
        IsDragElementProperty.Changed.AddClassHandler<AvaloniaObject>(OnIsDragElementChanged);
        IgnoreAltF4Property.Changed.AddClassHandler<AvaloniaObject>(OnIgnoreAltF4Changed);
        ShowInTaskManagerProperty.Changed.AddClassHandler<AvaloniaObject>(OnShowInTaskManagerChanged);
        HideWhenClosingProperty.Changed.AddClassHandler<AvaloniaObject>(OnHideWhenClosingChanged);
    }
}