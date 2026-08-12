using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Styling;
using HandyControl.Tools;

namespace HandyControl.Controls;

public class SwitchPropertyEditor : PropertyEditorBase
{
    public override Control CreateElement(PropertyItem propertyItem) => new ToggleButton
    {
        Theme = ResourceHelper.GetResource<ControlTheme>("ToggleButtonSwitch"),
        HorizontalAlignment = HorizontalAlignment.Left,
        IsEnabled = !propertyItem.IsReadOnly
    };

    public override AvaloniaProperty GetDependencyProperty() => ToggleButton.IsCheckedProperty;
}
