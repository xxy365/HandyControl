using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControlDemo.Data;

namespace HandyControlDemo.ViewModel;

public partial class PropertyGridViewModel : ObservableObject
{
    [ObservableProperty] private PropertyGridDemoModel? _demoModel;

    public PropertyGridViewModel()
    {
        DemoModel = new PropertyGridDemoModel
        {
            String = "TestString",
            Enum = Gender.Female,
            Boolean = true,
            Integer = 98,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }
}
