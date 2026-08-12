using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace HandyControl.Controls;

public abstract class PropertyEditorBase
{
    public abstract Control CreateElement(PropertyItem propertyItem);

    public virtual void CreateBinding(PropertyItem propertyItem, Control element) =>
        element.Bind(GetDependencyProperty(),
            new Binding(propertyItem.PropertyName ?? string.Empty)
            {
                Source = propertyItem.Value,
                Mode = GetBindingMode(propertyItem),
                UpdateSourceTrigger = GetUpdateSourceTrigger(propertyItem),
                Converter = GetConverter(propertyItem)
            });

    public abstract AvaloniaProperty GetDependencyProperty();

    public virtual BindingMode GetBindingMode(PropertyItem propertyItem) =>
        propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;

    public virtual UpdateSourceTrigger GetUpdateSourceTrigger(PropertyItem propertyItem) =>
        UpdateSourceTrigger.PropertyChanged;

    protected virtual IValueConverter? GetConverter(PropertyItem propertyItem) => null;
}
