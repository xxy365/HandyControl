using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControlDemo.Properties.Langs;
using MessageBox = HandyControl.Controls.MessageBox;

namespace HandyControlDemo.UserControl;

public partial class WindowDemo : Avalonia.Controls.UserControl
{
    public WindowDemo()
    {
        InitializeComponent();
    }

    private void ButtonCommonWindow_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = new HandyControl.Controls.Window
        {
            Title = Properties.Langs.Lang.Title,
            Width = 800,
            Height = 450,
            Content = new Border { Background = new SolidColorBrush(Color.Parse("#262e2f")) }
        };
        ShowWindow(window);
    }

    private void ButtonBlurWindow_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = new BlurWindow
        {
            Title = Properties.Langs.Lang.Title,
            Width = 800,
            Height = 450,
            Content = new Border { Background = Brushes.Transparent }
        };
        ShowWindow(window);
    }

    private void ButtonCustomTitleBar_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = new HandyControl.Controls.Window
        {
            Title = Properties.Langs.Lang.Title,
            Width = 800,
            Height = 450,
            CustomTitleBarContent = new TextBlock
            {
                Text = LangProvider.Instance.DragMe,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            },
            Content = new TextBlock
            {
                Text = LangProvider.Instance.CustomTitleBarWindow,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        ShowWindow(window);
    }

    private void ButtonMessage_OnClick(object? sender, RoutedEventArgs e)
    {
        MessageBox.Show(Properties.Langs.Lang.GrowlAsk, Properties.Langs.Lang.Title,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
    }

    private void ButtonCustomMessage_OnClick(object? sender, RoutedEventArgs e)
    {
        MessageBox.Show(new MessageBoxInfo
        {
            Message = Properties.Langs.Lang.GrowlAsk,
            Caption = Properties.Langs.Lang.Title,
            Button = MessageBoxButton.YesNo,
            IconBrushKey = ResourceToken.AccentBrush,
            IconKey = ResourceToken.AskGeometry,
            StyleKey = "MessageBoxCustom"
        });
    }

    private void ButtonCustomContent_OnClick(object? sender, RoutedEventArgs e)
    {
        var picker = new HandyControl.Controls.ColorPicker();
        var window = new PopupWindow
        {
            PopupElement = picker,
            Title = Properties.Langs.Lang.ColorPicker
        };
        picker.SelectedColorChanged += (_, _) => window.Close();
        picker.Canceled += (_, _) => window.Close();
        window.Show();
    }

    private void ButtonMouseFollow_OnClick(object? sender, RoutedEventArgs e)
    {
        var picker = new HandyControl.Controls.ColorPicker();
        var window = new PopupWindow
        {
            PopupElement = picker
        };
        picker.SelectedColorChanged += (_, _) => window.Close();
        picker.Canceled += (_, _) => window.Close();
        window.Show(ButtonMouseFollow, false);
    }

    private void ButtonNoCustomTitleBar_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = new HandyControl.Controls.Window
        {
            Title = Properties.Langs.Lang.OpenNoCustomTitleBarDragableWindow,
            Width = 800,
            Height = 450,
            ShowTitle = true,
            ShowCustomTitleBar = false,
            Content = new TextBlock
            {
                Text = Properties.Langs.Lang.DragHere,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 16, 0, 0)
            }
        };
        ShowWindow(window);
    }

    private void ShowWindow(Avalonia.Controls.Window window)
    {
        if (TopLevel.GetTopLevel(this) is Avalonia.Controls.Window owner)
        {
            window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }
    }
}