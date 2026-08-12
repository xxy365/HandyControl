using HandyControlDemo.Service;

namespace HandyControlDemo.ViewModel;

public class SideMenuViewModel : DemoViewModelBase<string>
{
    public SideMenuViewModel(DataService dataService)
    {
        DataList = dataService.GetSideMenuDemoDataList();
    }
}