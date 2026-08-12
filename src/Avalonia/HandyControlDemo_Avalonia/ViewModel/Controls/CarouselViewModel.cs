using HandyControlDemo.Service;

namespace HandyControlDemo.ViewModel;

public class CarouselViewModel : DemoViewModelBase<string>
{
    public CarouselViewModel(DataService dataService)
    {
        DataList = dataService.GetCarouselDemoDataList();
    }
}