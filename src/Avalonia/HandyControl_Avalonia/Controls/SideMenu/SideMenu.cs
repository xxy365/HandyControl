using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HandyControl.Data;

namespace HandyControl.Controls;

public class SideMenu : HeaderedSimpleItemsControl
{
    private SideMenuItem? _selectedItem;

    private SideMenuItem? _selectedHeader;

    private bool _isItemSelected;

    public SideMenu()
    {
        AddHandler(SideMenuItem.SelectedEvent, SideMenuItemSelected);
        Loaded += (s, e) => Init();
    }

    protected override void Refresh()
    {
        base.Refresh();
        Init();
    }

    private void Init()
    {
        if (ItemsHost == null) return;
        OnExpandModeChanged(ExpandMode);
    }

    private void SideMenuItemSelected(object? sender, RoutedEventArgs e)
    {
        if (e.Source is SideMenuItem item)
        {
            if (item.Role == SideMenuItemRole.Item)
            {
                _isItemSelected = true;

                if (Equals(item, _selectedItem)) return;

                if (_selectedItem != null)
                {
                    _selectedItem.IsSelected = false;
                }

                _selectedItem = item;
                _selectedItem.IsSelected = true;
                RaiseEvent(new FunctionEventArgs<object>(SelectionChangedEvent, this)
                {
                    Info = e.Source
                });
            }
            else
            {
                if (!Equals(item, _selectedHeader))
                {
                    if (_selectedHeader != null)
                    {
                        if (ExpandMode == ExpandMode.Freedom && item.ItemsHost!.IsVisible && !_isItemSelected)
                        {
                            item.IsSelected = false;
                            SwitchPanelArea(item);
                            return;
                        }

                        _selectedHeader.IsSelected = false;
                        if (ExpandMode != ExpandMode.Freedom)
                        {
                            SwitchPanelArea(_selectedHeader);
                        }
                    }

                    _selectedHeader = item;
                    _selectedHeader.IsSelected = true;
                    SwitchPanelArea(_selectedHeader);
                }
                else if (ExpandMode == ExpandMode.Freedom && !_isItemSelected)
                {
                    _selectedHeader.IsSelected = false;
                    SwitchPanelArea(_selectedHeader);
                    _selectedHeader = null;
                }

                if (_isItemSelected)
                {
                    _isItemSelected = false;
                }
                else if (_selectedHeader != null)
                {
                    if (AutoSelect)
                    {
                        if (_selectedItem != null)
                        {
                            _selectedItem.IsSelected = false;
                            _selectedItem = null;
                        }

                        _selectedHeader.SelectDefaultItem();
                    }
                    _isItemSelected = false;
                }

                if (!item.HasItems)
                {
                    RaiseEvent(new FunctionEventArgs<object>(SelectionChangedEvent, this)
                    {
                        Info = e.Source
                    });
                }
            }
        }
    }

    private void SwitchPanelArea(SideMenuItem oldItem)
    {
        switch (ExpandMode)
        {
            case ExpandMode.ShowAll:
                return;
            case ExpandMode.ShowOne:
            case ExpandMode.Freedom:
            case ExpandMode.Accordion:
                oldItem.SwitchPanelArea(oldItem.IsSelected);
                break;
        }
    }

    protected override Control GetContainerForItemOverride() => new SideMenuItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is SideMenuItem;

    public static readonly StyledProperty<bool> AutoSelectProperty =
        AvaloniaProperty.Register<SideMenu, bool>(nameof(AutoSelect), true);

    public bool AutoSelect
    {
        get => GetValue(AutoSelectProperty);
        set => SetValue(AutoSelectProperty, value);
    }

    public static readonly StyledProperty<ExpandMode> ExpandModeProperty =
        AvaloniaProperty.Register<SideMenu, ExpandMode>(nameof(ExpandMode));

    static SideMenu()
    {
        ExpandModeProperty.Changed.AddClassHandler<SideMenu>((s, e) =>
        {
            if (s.ItemsHost == null) return;
            s.OnExpandModeChanged((ExpandMode)e.NewValue!);
        });
    }

    private void OnExpandModeChanged(ExpandMode mode)
    {
        if (mode == ExpandMode.ShowAll)
        {
            ShowAll();
        }
        else if (mode == ExpandMode.ShowOne)
        {
            if (ItemsHost == null) return;
            SideMenuItem? sideMenuItemSelected = null;
            foreach (var sideMenuItem in ItemsHost.Children.OfType<SideMenuItem>())
            {
                if (sideMenuItemSelected != null)
                {
                    sideMenuItem.IsSelected = false;
                    if (sideMenuItem.ItemsHost != null)
                    {
                        foreach (var sideMenuSubItem in sideMenuItem.ItemsHost.Children.OfType<SideMenuItem>())
                        {
                            sideMenuSubItem.IsSelected = false;
                        }
                    }
                }
                else if (sideMenuItem.IsSelected)
                {
                    switch (sideMenuItem.Role)
                    {
                        case SideMenuItemRole.Header:
                            _selectedHeader = sideMenuItem;
                            break;
                        case SideMenuItemRole.Item:
                            _selectedItem = sideMenuItem;
                            break;
                    }

                    ShowSelectedOne(sideMenuItem);
                    sideMenuItemSelected = sideMenuItem;

                    if (sideMenuItem.ItemsHost != null)
                    {
                        foreach (var sideMenuSubItem in sideMenuItem.ItemsHost.Children.OfType<SideMenuItem>())
                        {
                            if (_selectedItem != null)
                            {
                                sideMenuSubItem.IsSelected = false;
                            }
                            else if (sideMenuSubItem.IsSelected)
                            {
                                _selectedItem = sideMenuSubItem;
                            }
                        }
                    }
                }
            }
        }
    }

    public ExpandMode ExpandMode
    {
        get => GetValue(ExpandModeProperty);
        set => SetValue(ExpandModeProperty, value);
    }

    public static readonly StyledProperty<double> PanelAreaLengthProperty =
        AvaloniaProperty.Register<SideMenu, double>(nameof(PanelAreaLength), double.NaN);

    public double PanelAreaLength
    {
        get => GetValue(PanelAreaLengthProperty);
        set => SetValue(PanelAreaLengthProperty, value);
    }

    private void ShowAll()
    {
        if (ItemsHost == null) return;
        foreach (var sideMenuItem in ItemsHost.Children.OfType<SideMenuItem>())
        {
            sideMenuItem.SwitchPanelArea(true);
        }
    }

    private void ShowSelectedOne(SideMenuItem item)
    {
        if (ItemsHost == null) return;
        foreach (var sideMenuItem in ItemsHost.Children.OfType<SideMenuItem>())
        {
            sideMenuItem.SwitchPanelArea(Equals(sideMenuItem, item));
        }
    }

    public static readonly RoutedEvent<FunctionEventArgs<object>> SelectionChangedEvent =
        RoutedEvent.Register<SideMenu, FunctionEventArgs<object>>("SelectionChanged", RoutingStrategies.Bubble);

    public event EventHandler<FunctionEventArgs<object>>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }
}