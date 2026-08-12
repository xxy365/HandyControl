using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace HandyControl.Controls;

public class HorizontalAlignmentPropertyEditor : PropertyEditorBase
{
    public override Control CreateElement(PropertyItem propertyItem) => new ComboBox
    {
        IsEnabled = !propertyItem.IsReadOnly,
        ItemsSource = Enum.GetValues(propertyItem.PropertyType!),
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
    };

    public override AvaloniaProperty GetDependencyProperty() => SelectingItemsControl.SelectedValueProperty;
}
