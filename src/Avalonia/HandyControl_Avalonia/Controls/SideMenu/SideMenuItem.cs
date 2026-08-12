using System;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using HandyControl.Data;

namespace HandyControl.Controls;

public class SideMenuItem : HeaderedSimpleItemsControl, ISelectable
{
    private bool _isPointerPressed;

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SideMenuItem, object?>(nameof(Icon));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<SideMenuItem, HorizontalAlignment>(nameof(HorizontalContentAlignment));

    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<SideMenuItem, VerticalAlignment>(nameof(VerticalContentAlignment));

    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    public SideMenuItem()
    {
        var expandModeBinding = new Binding(SideMenu.ExpandModeProperty.Name)
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
            {
                AncestorType = typeof(SideMenu)
            }
        };
        Bind(ExpandModeProperty, expandModeBinding);
    }

    internal static readonly StyledProperty<ExpandMode> ExpandModeProperty =
        AvaloniaProperty.Register<SideMenuItem, ExpandMode>(nameof(ExpandMode));

    internal ExpandMode ExpandMode
    {
        get => GetValue(ExpandModeProperty);
        set => SetValue(ExpandModeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (ItemsHost != null)
        {
            ItemsHost.IsVisible = ExpandMode == ExpandMode.ShowAll || IsSelected;
        }
    }

    protected override void Refresh()
    {
        if (ItemsHost == null) return;

        ItemsHost.Children.Clear();
        foreach (var item in Items)
        {
            if (IsItemItsOwnContainerOverride(item))
            {
                if (item is not Control control) continue;
                ItemsHost.Children.Add(control);
            }
            else
            {
                var container = GetContainerForItemOverride();
                PrepareContainerForItemOverride(container, item);
                ItemsHost.Children.Add(container);
            }
        }

        SwitchPanelArea(ExpandMode == ExpandMode.ShowAll || IsSelected);
    }

    protected virtual void OnSelected(RoutedEventArgs e)
    {
        RaiseEvent(e);

        if (Command == null) return;
        if (Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }
    }

    public static readonly RoutedEvent<RoutedEventArgs> SelectedEvent =
        RoutedEvent.Register<SideMenuItem, RoutedEventArgs>("Selected", RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Selected
    {
        add => AddHandler(SelectedEvent, value);
        remove => RemoveHandler(SelectedEvent, value);
    }

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<SideMenuItem, bool>(nameof(IsSelected));

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly StyledProperty<SideMenuItemRole> RoleProperty =
        AvaloniaProperty.Register<SideMenuItem, SideMenuItemRole>(nameof(Role));

    public SideMenuItemRole Role
    {
        get => GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    protected override Control GetContainerForItemOverride() => new SideMenuItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is SideMenuItem;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _isPointerPressed = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_isPointerPressed)
        {
            IsSelected = true;
            OnSelected(new RoutedEventArgs(SelectedEvent, this));
            _isPointerPressed = false;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _isPointerPressed = false;
    }

    internal void SelectDefaultItem()
    {
        if (Role == SideMenuItemRole.Header && ItemsHost != null && ItemsHost.Children.Count > 0)
        {
            var item = ItemsHost.Children.OfType<SideMenuItem>().FirstOrDefault();
            if (item is { IsSelected: false })
            {
                item.OnSelected(new RoutedEventArgs(SelectedEvent, item));
            }
        }
    }

    internal void SwitchPanelArea(bool isShow)
    {
        if (ItemsHost == null) return;
        if (Role == SideMenuItemRole.Header)
        {
            ItemsHost.IsVisible = isShow;
        }
    }

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<SideMenuItem, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<SideMenuItem, object?>(nameof(CommandParameter));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly StyledProperty<IInputElement?> CommandTargetProperty =
        AvaloniaProperty.Register<SideMenuItem, IInputElement?>(nameof(CommandTarget));

    public IInputElement? CommandTarget
    {
        get => GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }
}