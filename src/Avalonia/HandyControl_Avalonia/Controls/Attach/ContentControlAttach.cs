using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Reactive;

using HandyControl.Tools;

namespace HandyControl.Controls;

public class ContentControlAttach
{
    public static readonly AttachedProperty<string> ContentStringFormatProperty =
        AvaloniaProperty.RegisterAttached<ContentControlAttach, AvaloniaObject, string>("ContentStringFormat");

    public static void SetContentStringFormat(AvaloniaObject element, string value) =>
        element.SetValue(ContentStringFormatProperty, value);

    public static string GetContentStringFormat(AvaloniaObject element) =>
        element.GetValue(ContentStringFormatProperty);

    public static readonly AttachedProperty<object?> ContentProperty =
        AvaloniaProperty.RegisterAttached<ContentControlAttach, AvaloniaObject, object?>("Content");

    public static void SetContent(AvaloniaObject element, object? value) => element.SetValue(ContentProperty, value);

    public static object? GetContent(AvaloniaObject element) => element.GetValue(ContentProperty);

    static ContentControlAttach()
    {
        ContentStringFormatProperty.Changed.AddClassHandler<AvaloniaObject>(OnContentChanged);
        ContentProperty.Changed.AddClassHandler<AvaloniaObject>(OnContentChanged);
    }

    private static void OnContentChanged(AvaloniaObject element, AvaloniaPropertyChangedEventArgs e)
    {
        if (element is not ContentControl contentControl)
        {
            return;
        }

        contentControl.ClearValue(ContentControl.ContentProperty);

        var stringFormat = contentControl.GetValue(ContentStringFormatProperty);
        var source = contentControl.GetObservable(ContentProperty);
        if (string.IsNullOrEmpty(stringFormat))
        {
            contentControl.Bind(ContentControl.ContentProperty, source);
        }
        else
        {
            var template = stringFormat.Contains('{') ? stringFormat : "{0:" + stringFormat + "}";
            contentControl.Bind(ContentControl.ContentProperty,
                source.Select(value =>
                    value is string s ? s : string.Format(CultureInfo.CurrentCulture, template, value)));
        }
    }
}
