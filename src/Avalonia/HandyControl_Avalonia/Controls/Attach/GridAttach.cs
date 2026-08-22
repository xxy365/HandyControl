using System;
using Avalonia;
using Avalonia.Controls;

namespace HandyControl.Controls;

public class GridAttach
{
    public static readonly AttachedProperty<string?> NameProperty =
        AvaloniaProperty.RegisterAttached<GridAttach, AvaloniaObject, string?>("Name");

    public static void SetName(AvaloniaObject element, string? value) =>
        element.SetValue(NameProperty, value);

    public static string? GetName(AvaloniaObject element) =>
        element.GetValue(NameProperty);

    public static readonly AttachedProperty<string?> RowNameProperty =
        AvaloniaProperty.RegisterAttached<GridAttach, AvaloniaObject, string?>("RowName");

    private static void OnRowNameChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is Control control)
        {
            if (e.NewValue is string rowName && !string.IsNullOrEmpty(rowName))
            {
                if (control.Parent is Grid grid)
                {
                    for (var i = 0; i < grid.RowDefinitions.Count; i++)
                    {
                        var gridRowDefinition = grid.RowDefinitions[i];
                        var gridRowName = GetName(gridRowDefinition);

                        if (!string.IsNullOrEmpty(gridRowName) &&
                            gridRowName.Equals(rowName, StringComparison.Ordinal))
                        {
                            Grid.SetRow(control, i);
                            return;
                        }
                    }
                }
            }
        }
    }

    public static void SetRowName(AvaloniaObject element, string? value) =>
        element.SetValue(RowNameProperty, value);

    public static string? GetRowName(AvaloniaObject element) =>
        element.GetValue(RowNameProperty);

    public static readonly AttachedProperty<string?> ColumnNameProperty =
        AvaloniaProperty.RegisterAttached<GridAttach, AvaloniaObject, string?>("ColumnName");

    private static void OnColumnNameChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is Control control)
        {
            if (e.NewValue is string columnName && !string.IsNullOrEmpty(columnName))
            {
                if (control.Parent is Grid grid)
                {
                    for (var i = 0; i < grid.ColumnDefinitions.Count; i++)
                    {
                        var gridColumnDefinition = grid.ColumnDefinitions[i];
                        var gridColumnName = GetName(gridColumnDefinition);

                        if (!string.IsNullOrEmpty(gridColumnName) &&
                            gridColumnName.Equals(columnName, StringComparison.Ordinal))
                        {
                            Grid.SetColumn(control, i);
                            return;
                        }
                    }
                }
            }
        }
    }

    public static void SetColumnName(AvaloniaObject element, string? value) =>
        element.SetValue(ColumnNameProperty, value);

    public static string? GetColumnName(AvaloniaObject element) =>
        element.GetValue(ColumnNameProperty);

    static GridAttach()
    {
        RowNameProperty.Changed.AddClassHandler<AvaloniaObject>(OnRowNameChanged);
        ColumnNameProperty.Changed.AddClassHandler<AvaloniaObject>(OnColumnNameChanged);
    }
}