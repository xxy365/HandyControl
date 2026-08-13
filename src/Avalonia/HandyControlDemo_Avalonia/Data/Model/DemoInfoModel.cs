using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControlDemo.Properties.Langs;

namespace HandyControlDemo.Data;

public class DemoInfoModel : ObservableObject
{
    public string Key { get; set; } = string.Empty;

    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string DisplayTitle => LangProvider.Instance[Key];

    public void RefreshDisplay() => OnPropertyChanged(nameof(DisplayTitle));

    private int _selectedIndex;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    public bool IsGroupEnabled { get; set; }

    public IList<DemoItemModel> DemoItemList { get; set; } = [];
}
