using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControlDemo.Properties.Langs;

namespace HandyControlDemo.ViewModel;

public partial class GrowlDemoViewModel : ObservableObject
{
    private readonly string _token = "HandyControlDemo_Growl";

    public TransitionMode[] TransitionModes { get; } = Enum.GetValues<TransitionMode>();

    [ObservableProperty] private TransitionMode _transitionMode;

    public GrowlDemoViewModel()
    {
        _transitionMode = TransitionMode.Right2LeftWithFade;
        SetTransitionMode(_transitionMode);
    }

    partial void OnTransitionModeChanged(TransitionMode value) => SetTransitionMode(value);

    private void SetTransitionMode(TransitionMode value)
    {
        var window = GetMainWindow();
        if (window is not null)
        {
            Growl.SetTransitionMode(window, value);
        }
    }

    [RelayCommand]
    private void Info() => Growl.Info(Lang.GrowlInfo, _token);

    [RelayCommand]
    private void Success() => Growl.Success(Lang.GrowlSuccess, _token);

    [RelayCommand]
    private void Warning() => Growl.Warning(new GrowlInfo
    {
        Message = Lang.GrowlWarning,
        CancelStr = Lang.Ignore,
        ActionBeforeClose = isConfirmed =>
        {
            Growl.Info(isConfirmed.ToString());
            return true;
        },
        Token = _token
    });

    [RelayCommand]
    private void Error() => Growl.Error(Lang.GrowlError, _token);

    [RelayCommand]
    private void Ask() => Growl.Ask(Lang.GrowlAsk, isConfirmed =>
    {
        Growl.Info(isConfirmed.ToString());
        return true;
    }, _token);

    [RelayCommand]
    private void Fatal() => Growl.Fatal(new GrowlInfo
    {
        Message = Lang.GrowlFatal,
        ShowDateTime = false,
        Token = _token
    });

    [RelayCommand]
    private void Clear() => Growl.Clear(_token);

    [RelayCommand]
    private void InfoGlobal() => Growl.InfoGlobal(Lang.GrowlInfo);

    [RelayCommand]
    private void SuccessGlobal() => Growl.SuccessGlobal(Lang.GrowlSuccess);

    [RelayCommand]
    private void WarningGlobal() => Growl.WarningGlobal(new GrowlInfo
    {
        Message = Lang.GrowlWarning,
        CancelStr = Lang.Ignore,
        ActionBeforeClose = isConfirmed =>
        {
            Growl.InfoGlobal(isConfirmed.ToString());
            return true;
        }
    });

    [RelayCommand]
    private void ErrorGlobal() => Growl.ErrorGlobal(Lang.GrowlError);

    [RelayCommand]
    private void AskGlobal() => Growl.AskGlobal(Lang.GrowlAsk, isConfirmed =>
    {
        Growl.InfoGlobal(isConfirmed.ToString());
        return true;
    });

    [RelayCommand]
    private void FatalGlobal() => Growl.FatalGlobal(new GrowlInfo
    {
        Message = Lang.GrowlFatal,
        ShowDateTime = false
    });

    [RelayCommand]
    private void ClearGlobal() => Growl.ClearGlobal();

    private static Avalonia.Controls.Window? GetMainWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }
}
