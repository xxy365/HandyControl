using System.Collections.Generic;
using Avalonia.Interactivity;

namespace HandyControlDemo.UserControl;

public partial class CheckComboBoxDemo : Avalonia.Controls.UserControl
{
    private static readonly string[] DataList =
    {
        "C#", "F#", "VB.NET", "TypeScript", "JavaScript", "Python", "Go", "Rust", "Java", "Kotlin"
    };

    public CheckComboBoxDemo()
    {
        InitializeComponent();
        Loaded += CheckComboBoxDemo_Loaded;
    }

    private void CheckComboBoxDemo_Loaded(object? sender, System.EventArgs e)
    {
        CbBasic.ItemsSource = new List<string>(DataList);
        CbReadOnly.ItemsSource = new List<string>(DataList);
        CbDisabled.ItemsSource = new List<string>(DataList);
        CbSmall.ItemsSource = new List<string>(DataList);

        CbReadOnly.Selection.Select(0);
        CbReadOnly.Selection.Select(2);
        CbReadOnly.Selection.Select(4);
    }

    private void ButtonSelectAll_OnClick(object? sender, RoutedEventArgs e)
    {
        CbBasic.Selection.SelectAll();
    }

    private void ButtonClear_OnClick(object? sender, RoutedEventArgs e)
    {
        CbBasic.Selection.Clear();
    }
}
