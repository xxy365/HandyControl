using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace HandyControl.Controls;

public class ImagePropertyEditor : PropertyEditorBase
{
    public override Control CreateElement(PropertyItem propertyItem) => new ImageSelector
    {
        IsEnabled = !propertyItem.IsReadOnly,
        Width = 50,
        Height = 50,
        HorizontalAlignment = HorizontalAlignment.Left
    };

    public override AvaloniaProperty GetDependencyProperty() => ImageSelector.HasValueProperty;

    public override void CreateBinding(PropertyItem propertyItem, Control element)
    {
    }
}
