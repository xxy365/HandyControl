using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using HandyControl.Collections;

namespace HandyControl.Controls;

public class Transfer : ListBox
{
    private const string ElementSelectedListBox = "PART_SelectedListBox";

    private ListBox? _selectedListBox;

    public static readonly RoutedEvent<SelectionChangedEventArgs> TransferredItemsChangedEvent =
        RoutedEvent.Register<Transfer, SelectionChangedEventArgs>("TransferredItemsChanged", RoutingStrategies.Bubble);

    public event EventHandler<SelectionChangedEventArgs>? TransferredItemsChanged
    {
        add => AddHandler(TransferredItemsChangedEvent, value);
        remove => RemoveHandler(TransferredItemsChangedEvent, value);
    }

    private static readonly DirectProperty<Transfer, IList?> TransferredItemsProperty =
        AvaloniaProperty.RegisterDirect<Transfer, IList?>(nameof(TransferredItems),
            o => o.TransferredItems, null);

    private IList? _transferredItems;

    public IList? TransferredItems
    {
        get => _transferredItems;
        private set => SetAndRaise(TransferredItemsProperty, ref _transferredItems, value);
    }

    public Transfer()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        SelectItems();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _selectedListBox = e.NameScope.Find<ListBox>(ElementSelectedListBox);
        var selectButton = e.NameScope.Find<Button>("PART_SelectButton");
        if (selectButton != null) selectButton.Click += (_, _) => SelectItems();
        var deselectButton = e.NameScope.Find<Button>("PART_DeselectButton");
        if (deselectButton != null) deselectButton.Click += (_, _) => DeselectItems();
    }

    protected virtual void OnTransferredItemsChanged(SelectionChangedEventArgs e)
    {
        RaiseEvent(e);
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = item?.GetType() ?? typeof(object);
        return !(item is TransferItem);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new TransferItem();

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
    }

    private void SelectItems()
    {
        if (_selectedListBox == null || SelectedItems == null || SelectedItems.Count == 0) return;

        foreach (var item in SelectedItems)
        {
            if (ContainerFromItem(item) is not TransferItem { IsTransferred: false } selectedItem) continue;

            selectedItem.IsTransferred = true;

            var transferItem = new TransferItem
            {
                Tag = item,
                Content = item
            };

            _selectedListBox.Items.Add(transferItem);
        }

        SetTransferredItems(_selectedListBox.Items.OfType<TransferItem>().Select(item => item.Tag));
        OnTransferredItemsChanged(new SelectionChangedEventArgs(TransferredItemsChangedEvent, new List<object>(), SelectedItems.Cast<object>().ToList()));
        UnselectAll();
    }

    private void DeselectItems()
    {
        if (_selectedListBox == null) return;

        var deselectItems = new List<object>();
        foreach (var transferItem in _selectedListBox.Items.OfType<TransferItem>().ToList())
        {
            if (!transferItem.IsSelected) continue;
            if (transferItem.Tag is null || ContainerFromItem(transferItem.Tag) is not TransferItem selectedItem) continue;

            _selectedListBox.Items.Remove(transferItem);
            deselectItems.Add(transferItem.Tag);
            selectedItem.SetCurrentValue(TransferItem.IsTransferredProperty, false);
            selectedItem.SetCurrentValue(ListBoxItem.IsSelectedProperty, false);
        }

        SetTransferredItems(_selectedListBox.Items.OfType<TransferItem>().Select(item => item.Tag));
        OnTransferredItemsChanged(new SelectionChangedEventArgs(TransferredItemsChangedEvent, deselectItems, new List<object>()));
    }

    private void SetTransferredItems(IEnumerable selectedItems)
    {
        var oldSelectedItems = TransferredItems as ManualObservableCollection<object>;
        if (oldSelectedItems == null)
        {
            oldSelectedItems = new ManualObservableCollection<object>();
            TransferredItems = oldSelectedItems;
        }

        oldSelectedItems.CanNotify = false;
        oldSelectedItems.Clear();
        foreach (var selectedItem in selectedItems)
        {
            oldSelectedItems.Add(selectedItem);
        }
        oldSelectedItems.CanNotify = true;
    }
}