using Avalonia;
using Avalonia.Controls;

namespace HandyControl.Controls;

public class DatePropertyEditor : PropertyEditorBase
{
    public override Control CreateElement(PropertyItem propertyItem) => new DateTimePicker
    {
        IsEnabled = !propertyItem.IsReadOnly
    };

    public override AvaloniaProperty GetAvaloniaProperty() => DateTimePicker.SelectedDateTimeProperty;
}
