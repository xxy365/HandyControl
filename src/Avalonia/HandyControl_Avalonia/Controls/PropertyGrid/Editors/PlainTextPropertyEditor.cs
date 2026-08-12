using Avalonia;
using Avalonia.Controls;

namespace HandyControl.Controls;

public class PlainTextPropertyEditor : PropertyEditorBase
{
    public override Control CreateElement(PropertyItem propertyItem) => new TextBox
    {
        IsReadOnly = propertyItem.IsReadOnly
    };

    public override AvaloniaProperty GetDependencyProperty() => TextBox.TextProperty;
}
