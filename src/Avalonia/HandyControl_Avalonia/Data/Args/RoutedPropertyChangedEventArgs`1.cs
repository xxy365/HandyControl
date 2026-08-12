using Avalonia.Interactivity;

namespace HandyControl.Data;

public class RoutedPropertyChangedEventArgs<T> : RoutedEventArgs
{
    public RoutedPropertyChangedEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public RoutedPropertyChangedEventArgs(T oldValue, T newValue, RoutedEvent routedEvent) : base(routedEvent)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public T OldValue { get; }

    public T NewValue { get; }
}
