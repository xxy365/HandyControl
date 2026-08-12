using System;
using Avalonia.Interactivity;

namespace HandyControl.Controls;

public interface ISelectable
{
    event EventHandler<RoutedEventArgs>? Selected;

    bool IsSelected { get; set; }
}