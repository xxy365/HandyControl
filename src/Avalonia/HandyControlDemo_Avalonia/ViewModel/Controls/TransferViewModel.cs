using System.Collections.ObjectModel;
using HandyControlDemo.Data;
using HandyControlDemo.Service;

namespace HandyControlDemo.ViewModel;

public class TransferViewModel : DemoViewModelBase<DemoDataModel>
{
    public TransferViewModel(DataService dataService)
    {
        DataList = new ObservableCollection<DemoDataModel>(dataService.GetTransferDemoDataList());
    }
}