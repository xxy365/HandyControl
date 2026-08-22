using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using HandyControlDemo.Data;
using HandyControlDemo.Tools;
using HandyControlDemo.ViewModel;

namespace HandyControlDemo.UserControl;

public partial class LeftMainContent : Avalonia.Controls.UserControl
{
    public LeftMainContent()
    {
        InitializeComponent();
    }

    private void TabControl_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        DebugLog.Log($"LeftMainContent.SelectionChanged: added={e.AddedItems.Count}, removed={e.RemovedItems.Count}");

        foreach (var item in e.AddedItems)
        {
            switch (item)
            {
                case DemoItemModel demoItem:
                    DebugLog.Log($"LeftMainContent: switch demo -> {demoItem.TargetCtlName}");
                    WeakReferenceMessenger.Default.Send(demoItem, MessageToken.SwitchDemo);
                    break;

                case DemoInfoModel demoInfo:
                    SetDemoInfoCurrent(demoInfo);
                    break;

                case Control { DataContext: DemoInfoModel fromContainer }:
                    SetDemoInfoCurrent(fromContainer);
                    break;

                default:
                    DebugLog.Log($"LeftMainContent: unknown added item type: {item?.GetType().FullName ?? "null"}");
                    break;
            }
        }
    }

    private static void SetDemoInfoCurrent(DemoInfoModel demoInfo)
    {
        DebugLog.Log($"LeftMainContent: DemoInfoCurrent = {demoInfo.Key}");
        ViewModelLocator.Instance.Main.DemoInfoCurrent = demoInfo;
    }
}
