using CommunityToolkit.Mvvm.ComponentModel;
using HandyControlDemo.Properties.Langs;

namespace HandyControlDemo.Data;

public class DemoItemModel : ObservableObject
{
    private bool _isVisible = true;
    private string _queriesText = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DisplayName => LangProvider.Instance[Name];

    public void RefreshDisplay() => OnPropertyChanged(nameof(DisplayName));

    public string GroupName { get; set; } = string.Empty;

    public string TargetCtlName { get; set; } = string.Empty;

    public object? ImageBrush { get; set; }

    public bool IsNew { get; set; }

    public string QueriesText
    {
        get => _queriesText;
        set => SetProperty(ref _queriesText, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }
}
