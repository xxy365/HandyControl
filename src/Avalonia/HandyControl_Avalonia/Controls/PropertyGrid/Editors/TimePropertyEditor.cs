using Avalonia;
using Avalonia.Controls;

namespace HandyControl.Controls;

public class TimePropertyEditor : PropertyEditorBase
{
    public override Control CreateElement(PropertyItem propertyItem) => new TimePicker
    {
        IsEnabled = !propertyItem.IsReadOnly
    };

    public override AvaloniaProperty GetDependencyProperty() => TimePicker.SelectedTimeProperty;
}
