using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using HandyControl.Data;

namespace HandyControl.Controls;

[TemplatePart(ElementItemsControl, typeof(ItemsControl))]
[TemplatePart(ElementSearchBar, typeof(SearchBar))]
public class PropertyGrid : TemplatedControl
{
    private const string ElementItemsControl = "PART_ItemsControl";

    private const string ElementSearchBar = "PART_SearchBar";

    private ItemsControl? _itemsControl;

    private SearchBar? _searchBar;

    private readonly ObservableCollection<object> _items = [];

    private string? _searchKey;

    private bool _isGrouped;

    public PropertyGrid()
    {
        UpdateItems(SelectedObject);
    }

    public virtual PropertyResolver PropertyResolver { get; } = new();

    public static readonly RoutedEvent<RoutedPropertyChangedEventArgs<object?>> SelectedObjectChangedEvent =
        RoutedEvent.Register<PropertyGrid, RoutedPropertyChangedEventArgs<object?>>(nameof(SelectedObjectChanged), RoutingStrategies.Bubble);

    public event EventHandler<RoutedPropertyChangedEventArgs<object?>> SelectedObjectChanged
    {
        add => AddHandler(SelectedObjectChangedEvent, value);
        remove => RemoveHandler(SelectedObjectChangedEvent, value);
    }

    public static readonly StyledProperty<object?> SelectedObjectProperty =
        AvaloniaProperty.Register<PropertyGrid, object?>(nameof(SelectedObject));

    public object? SelectedObject
    {
        get => GetValue(SelectedObjectProperty);
        set => SetValue(SelectedObjectProperty, value);
    }

    protected virtual void OnSelectedObjectChanged(object? oldValue, object? newValue)
    {
        UpdateItems(newValue);
        RaiseEvent(new RoutedPropertyChangedEventArgs<object?>(oldValue, newValue, SelectedObjectChangedEvent));
    }

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<PropertyGrid, string?>(nameof(Description));

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly StyledProperty<double> MaxTitleWidthProperty =
        AvaloniaProperty.Register<PropertyGrid, double>(nameof(MaxTitleWidth));

    public double MaxTitleWidth
    {
        get => GetValue(MaxTitleWidthProperty);
        set => SetValue(MaxTitleWidthProperty, value);
    }

    public static readonly StyledProperty<double> MinTitleWidthProperty =
        AvaloniaProperty.Register<PropertyGrid, double>(nameof(MinTitleWidth));

    public double MinTitleWidth
    {
        get => GetValue(MinTitleWidthProperty);
        set => SetValue(MinTitleWidthProperty, value);
    }

    public static readonly StyledProperty<bool> ShowSortButtonProperty =
        AvaloniaProperty.Register<PropertyGrid, bool>(nameof(ShowSortButton), true);

    public bool ShowSortButton
    {
        get => GetValue(ShowSortButtonProperty);
        set => SetValue(ShowSortButtonProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_searchBar != null)
        {
            _searchBar.SearchStarted -= SearchBar_SearchStarted;
        }

        base.OnApplyTemplate(e);

        _itemsControl = e.NameScope.Find<ItemsControl>(ElementItemsControl);
        _searchBar = e.NameScope.Find<SearchBar>(ElementSearchBar);

        if (_itemsControl != null)
        {
            _itemsControl.ItemsSource = _items;
        }

        if (_searchBar != null)
        {
            _searchBar.SearchStarted += SearchBar_SearchStarted;
        }

        UpdateItems(SelectedObject);
    }

    private void UpdateItems(object? obj)
    {
        if (obj == null) return;

        _items.Clear();

        foreach (var item in TypeDescriptor.GetProperties(obj.GetType()).OfType<PropertyDescriptor>()
                     .Where(item => PropertyResolver.ResolveIsBrowsable(item)).Select(CreatePropertyItem))
        {
            item.InitElement();
            _items.Add(item);
        }

        if (_isGrouped)
        {
            SortByCategory();
        }
        else
        {
            SortByName();
        }

        if (_itemsControl != null)
        {
            _itemsControl.ItemsSource = null;
            _itemsControl.ItemsSource = _items;
        }
    }

    public void SortByCategory()
    {
        _isGrouped = true;

        var propertyItems = _items.OfType<PropertyItem>().ToList();
        _items.Clear();

        foreach (var group in propertyItems.GroupBy(item => item.Category ?? string.Empty).OrderBy(item => item.Key))
        {
            _items.Add(new PropertyGroupHeader
            {
                Header = group.Key
            });

            foreach (var item in group.OrderBy(item => item.DisplayName))
            {
                _items.Add(item);
            }
        }
    }

    public void SortByName()
    {
        _isGrouped = false;

        var propertyItems = _items.OfType<PropertyItem>().OrderBy(item => item.PropertyName).ToList();
        _items.Clear();

        foreach (var item in propertyItems)
        {
            _items.Add(item);
        }
    }

    private void SearchBar_SearchStarted(object? sender, FunctionEventArgs<string?> e)
    {
        _searchKey = e.Info;
        if (string.IsNullOrEmpty(_searchKey))
        {
            foreach (var item in _items.OfType<PropertyItem>())
            {
                item.Show();
            }
        }
        else
        {
            var searchKey = _searchKey.ToLower();
            foreach (var item in _items.OfType<PropertyItem>())
            {
                item.Show(item.PropertyName != null && item.PropertyName.ToLower().Contains(searchKey)
                          || item.DisplayName != null && item.DisplayName.ToLower().Contains(searchKey));
            }
        }
    }

    protected virtual PropertyItem CreatePropertyItem(PropertyDescriptor propertyDescriptor) => new()
    {
        Category = PropertyResolver.ResolveCategory(propertyDescriptor),
        DisplayName = PropertyResolver.ResolveDisplayName(propertyDescriptor),
        Description = PropertyResolver.ResolveDescription(propertyDescriptor),
        IsReadOnly = PropertyResolver.ResolveIsReadOnly(propertyDescriptor),
        DefaultValue = PropertyResolver.ResolveDefaultValue(propertyDescriptor),
        Editor = PropertyResolver.ResolveEditor(propertyDescriptor),
        Value = SelectedObject,
        PropertyName = propertyDescriptor.Name,
        PropertyType = propertyDescriptor.PropertyType,
        PropertyTypeName = $"{propertyDescriptor.PropertyType.Namespace}.{propertyDescriptor.PropertyType.Name}"
    };

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedObjectProperty)
        {
            OnSelectedObjectChanged(change.OldValue, change.NewValue);
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        TitleElement.SetTitleWidth(this, new GridLength(Math.Max(MinTitleWidth, Math.Min(MaxTitleWidth, Bounds.Width / 3))));
    }
}
