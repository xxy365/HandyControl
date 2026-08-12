using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace HandyControl.Controls;

public class SelectableItem : ContentControl, ISelectable
{
    private bool _isPointerPressed;

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        _isPointerPressed = false;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPointerPressed = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isPointerPressed)
        {
            if (SelfManage)
            {
                if (!IsSelected)
                {
                    IsSelected = true;
                    OnSelected(new RoutedEventArgs(SelectedEvent, this));
                }
                else if (CanDeselect)
                {
                    IsSelected = false;
                    OnSelected(new RoutedEventArgs(DeselectedEvent, this));
                }
            }
            else
            {
                if (CanDeselect)
                {
                    OnSelected(IsSelected
                        ? new RoutedEventArgs(DeselectedEvent, this)
                        : new RoutedEventArgs(SelectedEvent, this));
                }
                else
                {
                    OnSelected(new RoutedEventArgs(SelectedEvent, this));
                }
            }

            _isPointerPressed = false;
        }
    }

    protected virtual void OnSelected(RoutedEventArgs e) => RaiseEvent(e);

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<SelectableItem, bool>(nameof(IsSelected));

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly StyledProperty<bool> SelfManageProperty =
        AvaloniaProperty.Register<SelectableItem, bool>(nameof(SelfManage));

    public bool SelfManage
    {
        get => GetValue(SelfManageProperty);
        set => SetValue(SelfManageProperty, value);
    }

    public static readonly StyledProperty<bool> CanDeselectProperty =
        AvaloniaProperty.Register<SelectableItem, bool>(nameof(CanDeselect));

    public bool CanDeselect
    {
        get => GetValue(CanDeselectProperty);
        set => SetValue(CanDeselectProperty, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> SelectedEvent =
        RoutedEvent.Register<SelectableItem, RoutedEventArgs>(nameof(Selected), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Selected
    {
        add => AddHandler(SelectedEvent, value);
        remove => RemoveHandler(SelectedEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> DeselectedEvent =
        RoutedEvent.Register<SelectableItem, RoutedEventArgs>(nameof(Deselected), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Deselected
    {
        add => AddHandler(DeselectedEvent, value);
        remove => RemoveHandler(DeselectedEvent, value);
    }
}