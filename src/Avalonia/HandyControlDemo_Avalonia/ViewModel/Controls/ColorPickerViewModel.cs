using HandyControlDemo.Service;

namespace HandyControlDemo.ViewModel;

public class ColorPickerViewModel : DemoViewModelBase<string>
{
    public ColorPickerViewModel(DataService dataService)
    {
        DataList = dataService.GetColorPickerDemoDataList();
    }
}