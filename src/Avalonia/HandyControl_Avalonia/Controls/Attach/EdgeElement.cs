using Avalonia;

namespace HandyControl.Controls;

public class EdgeElement
{
    public static readonly AttachedProperty<object?> LeftContentProperty =
        AvaloniaProperty.RegisterAttached<EdgeElement, AvaloniaObject, object?>("LeftContent",
            inherits: false);

    public static void SetLeftContent(AvaloniaObject element, object? value) =>
        element.SetValue(LeftContentProperty, value);

    public static object? GetLeftContent(AvaloniaObject element) =>
        element.GetValue(LeftContentProperty);

    public static readonly AttachedProperty<object?> TopContentProperty =
        AvaloniaProperty.RegisterAttached<EdgeElement, AvaloniaObject, object?>("TopContent");

    public static void SetTopContent(AvaloniaObject element, object? value) =>
        element.SetValue(TopContentProperty, value);

    public static object? GetTopContent(AvaloniaObject element) =>
        element.GetValue(TopContentProperty);

    public static readonly AttachedProperty<object?> RightContentProperty =
        AvaloniaProperty.RegisterAttached<EdgeElement, AvaloniaObject, object?>("RightContent");

    public static void SetRightContent(AvaloniaObject element, object? value) =>
        element.SetValue(RightContentProperty, value);

    public static object? GetRightContent(AvaloniaObject element) =>
        element.GetValue(RightContentProperty);

    public static readonly AttachedProperty<object?> BottomContentProperty =
        AvaloniaProperty.RegisterAttached<EdgeElement, AvaloniaObject, object?>("BottomContent");

    public static void SetBottomContent(AvaloniaObject element, object? value) =>
        element.SetValue(BottomContentProperty, value);

    public static object? GetBottomContent(AvaloniaObject element) =>
        element.GetValue(BottomContentProperty);

    public static readonly AttachedProperty<bool> ShowEdgeContentProperty =
        AvaloniaProperty.RegisterAttached<EdgeElement, AvaloniaObject, bool>("ShowEdgeContent");

    public static void SetShowEdgeContent(AvaloniaObject element, bool value) =>
        element.SetValue(ShowEdgeContentProperty, value);

    public static bool GetShowEdgeContent(AvaloniaObject element) =>
        element.GetValue(ShowEdgeContentProperty);

    static EdgeElement()
    {
        LeftContentProperty.Changed.AddClassHandler<AvaloniaObject>(OnEdgeContentChanged);
        TopContentProperty.Changed.AddClassHandler<AvaloniaObject>(OnEdgeContentChanged);
        RightContentProperty.Changed.AddClassHandler<AvaloniaObject>(OnEdgeContentChanged);
        BottomContentProperty.Changed.AddClassHandler<AvaloniaObject>(OnEdgeContentChanged);
    }

    private static void OnEdgeContentChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e) =>
        SetShowEdgeContent(d, GetLeftContent(d) != null || GetTopContent(d) != null ||
                              GetRightContent(d) != null || GetBottomContent(d) != null);
}