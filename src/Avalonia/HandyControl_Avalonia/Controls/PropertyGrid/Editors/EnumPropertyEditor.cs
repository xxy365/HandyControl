using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace HandyControl.Controls;

public class EnumPropertyEditor : PropertyEditorBase
{
    public override Control CreateElement(PropertyItem propertyItem) => new ComboBox
    {
        IsEnabled = !propertyItem.IsReadOnly,
        ItemsSource = Enum.GetValues(propertyItem.PropertyType!)
    };

    public override AvaloniaProperty GetDependencyProperty() => SelectingItemsControl.SelectedValueProperty;
}
