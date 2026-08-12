using Avalonia;
using Avalonia.Controls;

namespace HandyControl.Controls;

public class DateTimePropertyEditor : PropertyEditorBase
{
    public override Control CreateElement(PropertyItem propertyItem) => new DateTimePicker
    {
        IsEnabled = !propertyItem.IsReadOnly
    };

    public override AvaloniaProperty GetDependencyProperty() => DateTimePicker.SelectedDateTimeProperty;
}
