using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace HandyControl.Controls;

public class ListBoxAttach
{
    public static readonly AttachedProperty<IList?> SelectedItemsProperty =
        AvaloniaProperty.RegisterAttached<ListBoxAttach, AvaloniaObject, IList?>("SelectedItems",
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static void SetSelectedItems(AvaloniaObject element, IList? value) =>
        element.SetValue(SelectedItemsProperty, value);

    public static IList? GetSelectedItems(AvaloniaObject element) =>
        element.GetValue(SelectedItemsProperty);

    internal static readonly AttachedProperty<bool> InternalActionProperty =
        AvaloniaProperty.RegisterAttached<ListBoxAttach, AvaloniaObject, bool>("InternalAction");

    internal static void SetInternalAction(AvaloniaObject element, bool value) =>
        element.SetValue(InternalActionProperty, value);

    internal static bool GetInternalAction(AvaloniaObject element) =>
        element.GetValue(InternalActionProperty);

    static ListBoxAttach()
    {
        SelectedItemsProperty.Changed.AddClassHandler<AvaloniaObject>(OnSelectedItemsChanged);
    }

    private static void OnSelectedItemsChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox)
        {
            return;
        }

        if (GetInternalAction(listBox))
        {
            return;
        }

        listBox.RemoveHandler(SelectingItemsControl.SelectionChangedEvent, OnListBoxSelectionChanged);
        listBox.SelectedItems?.Clear();

        if (e.NewValue is IList selectedItems)
        {
            foreach (var selectedItem in selectedItems)
            {
                listBox.SelectedItems?.Add(selectedItem);
            }
        }

        listBox.AddHandler(SelectingItemsControl.SelectionChangedEvent, OnListBoxSelectionChanged);
    }

    private static void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox)
        {
            SetInternalAction(listBox, true);
            SetSelectedItems(listBox, listBox.SelectedItems?.Cast<object>().ToArray());
            SetInternalAction(listBox, false);
        }
    }
}