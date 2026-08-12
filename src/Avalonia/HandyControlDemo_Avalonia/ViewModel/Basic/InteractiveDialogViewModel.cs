using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;

namespace HandyControlDemo.ViewModel;

public partial class InteractiveDialogViewModel : ObservableObject, IDialogResultable<string>
{
    public Action CloseAction { get; set; } = null!;

    [ObservableProperty]
    private string _result = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [RelayCommand]
    private void Close() => CloseAction?.Invoke();
}
