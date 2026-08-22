using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace HandyControl.Controls;

public class MenuItemAttach
{
    public static readonly AttachedProperty<string> GroupNameProperty =
        AvaloniaProperty.RegisterAttached<MenuItemAttach, AvaloniaObject, string>("GroupName", string.Empty);

    public static string GetGroupName(AvaloniaObject obj) => obj.GetValue(GroupNameProperty);

    public static void SetGroupName(AvaloniaObject obj, string value) =>
        obj.SetValue(GroupNameProperty, value);

    private static void OnGroupNameChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not MenuItem menuItem)
        {
            return;
        }

        menuItem.RemoveHandler(MenuItem.ClickEvent, MenuItem_Click);
        menuItem.PropertyChanged -= MenuItem_IsCheckedChanged;

        if (string.IsNullOrWhiteSpace(e.NewValue?.ToString()))
        {
            return;
        }

        menuItem.AddHandler(MenuItem.ClickEvent, MenuItem_Click);
        menuItem.PropertyChanged += MenuItem_IsCheckedChanged;
    }

    private static void MenuItem_IsCheckedChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not MenuItem menuItem || e.Property != MenuItem.IsCheckedProperty)
        {
            return;
        }

        if (e.NewValue is true && menuItem.Parent is MenuItem parent)
        {
            var groupName = GetGroupName(menuItem);
            parent
                .Items
                .OfType<MenuItem>()
                .Where(item => item != menuItem && item.ToggleType != MenuItemToggleType.None &&
                               string.Equals(GetGroupName(item), groupName))
                .ToList()
                .ForEach(item => item.SetCurrentValue(MenuItem.IsCheckedProperty, false));
        }
    }

    private static void MenuItem_Click(object? sender, RoutedEventArgs e)
    {
        // prevent uncheck when click the checked menu item
        if (e.Source is MenuItem { IsChecked: false } menuItem)
        {
            menuItem.IsChecked = true;
        }
    }

    static MenuItemAttach()
    {
        GroupNameProperty.Changed.AddClassHandler<AvaloniaObject>(OnGroupNameChanged);
    }
}