using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace HandyControl.Controls;

public class CoverView : TemplatedControl
{
    private const string ElementPanel = "PART_Panel";

    private readonly CoverViewContent _viewContent;

    private CoverViewItem? _selectedItem;

    private IEnumerable? _itemsSourceInternal;

    private readonly Dictionary<object, CoverViewItem?> _entryDic = new();

    private Panel? _panel;

    private Collection<object>? _items;

    public CoverView()
    {
        _viewContent = new CoverViewContent();

        AddHandler(SelectableItem.SelectedEvent, CoverViewItem_OnSelected);
    }

    private void CoverViewItem_OnSelected(object? sender, RoutedEventArgs e)
    {
        if (e.Source is CoverViewItem item)
        {
            if (_selectedItem == null)
            {
                item.IsSelected = true;
                _selectedItem = item;
                _viewContent.Content = item.Content;
                _viewContent.ContentTemplate = ItemTemplate;
                UpdateCoverViewContent(true);
                return;
            }

            if (!Equals(_selectedItem, item))
            {
                _selectedItem.IsSelected = false;
                item.IsSelected = true;
                _selectedItem = item;
                _viewContent.Content = item.Content;
                UpdateCoverViewContent(true);
                return;
            }

            _viewContent.Content = null;
            _viewContent.ContentTemplate = null;
            UpdateCoverViewContent(false);
            _selectedItem.IsSelected = false;
            _selectedItem = null;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _panel?.Children.Clear();
        _panel = e.NameScope.Find<WrapPanel>(ElementPanel);

        _viewContent.ContentHeight = ItemContentHeight;
        _viewContent.IsVisible = false;

        if (_selectedItem != null)
        {
            UpdateCoverViewContent(_selectedItem != null);
        }

        Refresh();
    }

    private void SetBindingForItem(CoverViewItem element)
    {
        element.Margin = ItemMargin;
        element.Width = ItemWidth;
        element.Height = ItemHeight;
        element.HeaderTemplate = ItemHeaderTemplate;
    }

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<CoverView, IEnumerable?>(nameof(ItemsSource));

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<CoverView, IDataTemplate?>(nameof(ItemTemplate));

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly StyledProperty<IStyle?> ItemContainerStyleProperty =
        AvaloniaProperty.Register<CoverView, IStyle?>(nameof(ItemContainerStyle));

    public IStyle? ItemContainerStyle
    {
        get => GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> ItemHeaderTemplateProperty =
        AvaloniaProperty.Register<CoverView, IDataTemplate?>(nameof(ItemHeaderTemplate));

    public IDataTemplate? ItemHeaderTemplate
    {
        get => GetValue(ItemHeaderTemplateProperty);
        set => SetValue(ItemHeaderTemplateProperty, value);
    }

    public static readonly StyledProperty<IStyle?> CoverViewContentStyleProperty =
        AvaloniaProperty.Register<CoverView, IStyle?>(nameof(CoverViewContentStyle));

    public IStyle? CoverViewContentStyle
    {
        get => GetValue(CoverViewContentStyleProperty);
        set => SetValue(CoverViewContentStyleProperty, value);
    }

    public static readonly StyledProperty<double> ItemContentHeightProperty =
        AvaloniaProperty.Register<CoverView, double>(nameof(ItemContentHeight), 300.0);

    public double ItemContentHeight
    {
        get => GetValue(ItemContentHeightProperty);
        set => SetValue(ItemContentHeightProperty, value);
    }

    public static readonly StyledProperty<bool> ItemContentHeightFixedProperty =
        AvaloniaProperty.Register<CoverView, bool>(nameof(ItemContentHeightFixed), true);

    public bool ItemContentHeightFixed
    {
        get => GetValue(ItemContentHeightFixedProperty);
        set => SetValue(ItemContentHeightFixedProperty, value);
    }

    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<CoverView, double>(nameof(ItemWidth), 200.0);

    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<CoverView, double>(nameof(ItemHeight), 200.0);

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public static readonly StyledProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.Register<CoverView, Thickness>(nameof(ItemMargin), new Thickness(8));

    public Thickness ItemMargin
    {
        get => GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }

    public static readonly StyledProperty<int> GroupsProperty =
        AvaloniaProperty.Register<CoverView, int>(nameof(Groups), 5);

    public int Groups
    {
        get => GetValue(GroupsProperty);
        set => SetValue(GroupsProperty, value);
    }

    public static readonly StyledProperty<bool> HasItemsProperty =
        AvaloniaProperty.Register<CoverView, bool>(nameof(HasItems));

    public bool HasItems => GetValue(HasItemsProperty);

    internal Collection<object>? Items => _items;

    private int GroupsEffective
    {
        get
        {
            if (double.IsNaN(Width) || Width <= 0)
            {
                return Groups;
            }

            var unit = ItemWidth + ItemMargin.Left + ItemMargin.Right;
            var g = (int)(Width / unit);
            return g < 1 ? 1 : g;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            ItemsSourceChanged(change.GetOldValue<IEnumerable>(), change.GetNewValue<IEnumerable>());
        }
        else if (change.Property == ItemWidthProperty || change.Property == ItemHeightProperty ||
                 change.Property == ItemMarginProperty || change.Property == ItemHeaderTemplateProperty)
        {
            foreach (var entry in _entryDic)
            {
                if (entry.Value is CoverViewItem item)
                {
                    SetBindingForItem(item);
                }
            }
        }
        else if (change.Property == ItemTemplateProperty || change.Property == ItemContainerStyleProperty)
        {
            Refresh();
        }
    }

    private void ItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue)
    {
        if (oldValue is INotifyCollectionChanged oldNotify)
        {
            oldNotify.CollectionChanged -= InternalCollectionChanged;
        }

        ClearItems();
        _itemsSourceInternal = newValue;

        if (newValue is INotifyCollectionChanged newNotify)
        {
            newNotify.CollectionChanged += InternalCollectionChanged;
        }

        if (newValue != null)
        {
            var items = new ObservableCollection<object>();
            foreach (var item in newValue)
            {
                items.Add(item);
            }
            _items = items;
        }
        else
        {
            _items = null;
        }

        Refresh();
    }

    private void InternalCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_panel == null) return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                ClearItems();
                break;
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        InsertItem(e.NewStartingIndex + i, e.NewItems[i]!);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        RemoveItem(item!);
                    }
                }
                break;
        }

        GenerateIndex();
        if (_viewContent.IsVisible)
        {
            UpdateCoverViewContentPosition();
        }
    }

    private void ClearItems()
    {
        _selectedItem = null;
        _viewContent.Content = null;
        _viewContent.ContentTemplate = null;
        _viewContent.IsVisible = false;

        if (_panel != null)
        {
            _panel.Children.Remove(_viewContent);
        }

        _entryDic.Clear();
        _items?.Clear();
    }

    private void RemoveItem(object item)
    {
        if (_panel == null) return;
        if (!_entryDic.TryGetValue(item, out var entry)) return;
        if (entry == null) return;

        if (ReferenceEquals(entry, _selectedItem))
        {
            _selectedItem = null;
            _viewContent.Content = null;
            _viewContent.IsVisible = false;
            _panel.Children.Remove(_viewContent);
        }

        _panel.Children.Remove(entry);
        _entryDic.Remove(item);
        _items?.Remove(item);
    }

    private void Refresh()
    {
        if (_panel == null) return;

        _panel.Children.Remove(_viewContent);
        _panel.Children.Clear();
        _entryDic.Clear();

        if (_items != null)
        {
            foreach (var item in _items)
            {
                AddItem(item);
            }
        }

        GenerateIndex();

        if (_selectedItem != null)
        {
            UpdateCoverViewContent(true);
        }
    }

    private void AddItem(object item)
    {
        if (_panel == null || _entryDic.ContainsKey(item)) return;

        var element = new CoverViewItem();
        SetBindingForItem(element);
        element.Header = item;
        element.Content = item;
        element.ContentTemplate = ItemTemplate;
        if (ItemContainerStyle != null)
        {
            element.Styles.Add(ItemContainerStyle);
        }

        _entryDic[item] = element;
        _panel.Children.Add(element);
    }

    private void InsertItem(int index, object item)
    {
        if (_panel == null || _entryDic.ContainsKey(item)) return;

        var element = new CoverViewItem();
        SetBindingForItem(element);
        element.Header = item;
        element.Content = item;
        element.ContentTemplate = ItemTemplate;

        _entryDic[item] = element;
        _panel.Children.Insert(index, element);
        _items?.Insert(index, item);
    }

    private void GenerateIndex()
    {
        var index = 0;
        foreach (var entry in _entryDic.Values)
        {
            if (entry != null)
            {
                entry.Index = index++;
            }
        }
    }

    private void UpdateCoverViewContent(bool isOpen)
    {
        if (_selectedItem == null || _panel == null) return;

        _viewContent.ContentHeight = ItemContentHeight;
        _viewContent.IsVisible = isOpen;

        _panel.Children.Remove(_viewContent);
        _panel.Children.Add(_viewContent);
        UpdateCoverViewContentPosition();
    }

    private void UpdateCoverViewContentPosition()
    {
        if (_selectedItem == null || _panel == null || !_viewContent.IsVisible) return;

        var total = _entryDic.Count + 1;
        var totalRow = total / Groups + (total % Groups > 0 ? 1 : 0);

        _panel.Children.Remove(_viewContent);

        if (total <= Groups)
        {
            _panel.Children.Add(_viewContent);
        }
        else
        {
            var row = _selectedItem.Index / Groups + 1;
            var insertIndex = row == totalRow
                ? _panel.Children.Count
                : row * Groups;
            if (insertIndex > _panel.Children.Count)
            {
                insertIndex = _panel.Children.Count;
            }
            _panel.Children.Insert(insertIndex, _viewContent);
        }

        _viewContent.UpdatePosition(_selectedItem.Index, Groups, ItemWidth);
    }
}