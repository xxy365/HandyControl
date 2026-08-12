using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HandyControlDemo.Data;
using HandyControlDemo.Properties.Langs;

namespace HandyControlDemo.UserControl;

public partial class StepBarDemo : Avalonia.Controls.UserControl
{
    public StepBarDemo()
    {
        InitializeComponent();

        DataContext = this;

        DataList = new List<StepBarDemoModel>
        {
            new() { Header = Lang.Step, Content = Lang.Register },
            new() { Header = Lang.Step, Content = Lang.BasicInfo },
            new() { Header = Lang.Step, Content = Lang.UploadFile },
            new() { Header = Lang.Step, Content = Lang.Complete }
        };
    }

    public List<StepBarDemoModel> DataList { get; }

    private void Prev_OnClick(object? sender, RoutedEventArgs e)
    {
        StepBarTop.Prev();
    }

    private void Next_OnClick(object? sender, RoutedEventArgs e)
    {
        StepBarTop.Next();
    }
}