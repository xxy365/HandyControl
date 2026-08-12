using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace HandyControl.Controls;

public class PropertyItem : ListBoxItem
{
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<PropertyItem, object?>(nameof(Value));

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<string?> DisplayNameProperty =
        AvaloniaProperty.Register<PropertyItem, string?>(nameof(DisplayName));

    public string? DisplayName
    {
        get => GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public static readonly StyledProperty<string?> PropertyNameProperty =
        AvaloniaProperty.Register<PropertyItem, string?>(nameof(PropertyName));

    public string? PropertyName
    {
        get => GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    public static readonly StyledProperty<Type?> PropertyTypeProperty =
        AvaloniaProperty.Register<PropertyItem, Type?>(nameof(PropertyType));

    public Type? PropertyType
    {
        get => GetValue(PropertyTypeProperty);
        set => SetValue(PropertyTypeProperty, value);
    }

    public static readonly StyledProperty<string?> PropertyTypeNameProperty =
        AvaloniaProperty.Register<PropertyItem, string?>(nameof(PropertyTypeName));

    public string? PropertyTypeName
    {
        get => GetValue(PropertyTypeNameProperty);
        set => SetValue(PropertyTypeNameProperty, value);
    }

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<PropertyItem, string?>(nameof(Description));

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<PropertyItem, bool>(nameof(IsReadOnly));

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly StyledProperty<object?> DefaultValueProperty =
        AvaloniaProperty.Register<PropertyItem, object?>(nameof(DefaultValue));

    public object? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public static readonly StyledProperty<string?> CategoryProperty =
        AvaloniaProperty.Register<PropertyItem, string?>(nameof(Category));

    public string? Category
    {
        get => GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    public static readonly StyledProperty<PropertyEditorBase?> EditorProperty =
        AvaloniaProperty.Register<PropertyItem, PropertyEditorBase?>(nameof(Editor));

    public PropertyEditorBase? Editor
    {
        get => GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    public static readonly StyledProperty<Control?> EditorElementProperty =
        AvaloniaProperty.Register<PropertyItem, Control?>(nameof(EditorElement));

    public Control? EditorElement
    {
        get => GetValue(EditorElementProperty);
        set => SetValue(EditorElementProperty, value);
    }

    public PropertyDescriptor? PropertyDescriptor { get; set; }

    public virtual void InitElement()
    {
        if (Editor == null) return;

        EditorElement = Editor.CreateElement(this);
        Editor.CreateBinding(this, EditorElement);
    }

    public void Show() => IsVisible = true;

    public void Hide() => IsVisible = false;

    public void Show(bool visible) => IsVisible = visible;
}
