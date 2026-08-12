using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace HandyControl.Controls;

public abstract class AdornerElement : TemplatedControl, IDisposable
{
    protected Control? ElementTarget { get; set; }

    public static readonly StyledProperty<Control?> TargetProperty =
        AvaloniaProperty.Register<AdornerElement, Control?>(nameof(Target));

    private static void OnTargetChanged(AdornerElement d, AvaloniaPropertyChangedEventArgs e)
    {
        d.OnTargetChanged(d.ElementTarget, false);
        d.OnTargetChanged(e.GetNewValue<Control?>(), true);
    }

    public Control? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public static readonly AttachedProperty<AdornerElement?> InstanceProperty =
        AvaloniaProperty.RegisterAttached<AdornerElement, AvaloniaObject, AdornerElement?>("Instance");

    private static void OnInstanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not Control target) return;
        var element = e.GetNewValue<AdornerElement?>();
        element?.OnInstanceChanged(target);
    }

    protected virtual void OnInstanceChanged(Control target) => Target = target;

    public static void SetInstance(AvaloniaObject element, AdornerElement? value) =>
        element.SetValue(InstanceProperty, value);

    public static AdornerElement? GetInstance(AvaloniaObject element) =>
        (AdornerElement?) element.GetValue(InstanceProperty);

    public static readonly AttachedProperty<bool> IsInstanceProperty =
        AvaloniaProperty.RegisterAttached<AdornerElement, AvaloniaObject, bool>("IsInstance", true);

    public static void SetIsInstance(AvaloniaObject element, bool value) =>
        element.SetValue(IsInstanceProperty, value);

    public static bool GetIsInstance(AvaloniaObject element) =>
        (bool) element.GetValue(IsInstanceProperty);

    protected virtual void OnTargetChanged(Control? element, bool isNew)
    {
        if (element == null) return;

        if (!isNew)
        {
            element.Unloaded -= TargetElement_Unloaded;
            ElementTarget = null;
        }
        else
        {
            element.Unloaded += TargetElement_Unloaded;
            ElementTarget = element;
        }
    }

    private void TargetElement_Unloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is Control element)
        {
            element.Unloaded -= TargetElement_Unloaded;
            Dispose();
        }
    }

    protected abstract void Dispose();

    void IDisposable.Dispose() => Dispose();

    static AdornerElement()
    {
        TargetProperty.Changed.AddClassHandler<AdornerElement>(OnTargetChanged);
        InstanceProperty.Changed.AddClassHandler<AvaloniaObject>(OnInstanceChanged);
    }
}
