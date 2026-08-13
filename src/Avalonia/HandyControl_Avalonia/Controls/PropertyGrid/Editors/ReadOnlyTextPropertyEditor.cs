using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using HandyControl.Tools;

namespace HandyControl.Controls;

public class ReadOnlyTextPropertyEditor : PropertyEditorBase
{
    public override Control CreateElement(PropertyItem propertyItem) => new TextBox
    {
        IsReadOnly = true
    };

    public override AvaloniaProperty GetAvaloniaProperty() => TextBox.TextProperty;

    public override BindingMode GetBindingMode(PropertyItem propertyItem) => BindingMode.OneWay;

    protected override IValueConverter? GetConverter(PropertyItem propertyItem) =>
        ResourceHelper.GetResource<IValueConverter>("Object2StringConverter");
}
