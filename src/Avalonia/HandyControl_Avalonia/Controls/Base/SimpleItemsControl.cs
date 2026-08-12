using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace HandyControl.Controls;

public class SimpleItemsControl : TemplatedControl, IAddChild
{
    private const string ElementPanel = "PART_Panel";

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<SimpleItemsControl, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<IStyle?> ItemContainerStyleProperty =
        AvaloniaProperty.Register<SimpleItemsControl, IStyle?>(nameof(ItemContainerStyle));

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SimpleItemsControl, IEnumerable?>(nameof(ItemsSource));

    public SimpleItemsControl()
    {
        var items = new ObservableCollection<object>();
        items.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                HasItems = true;
            }
            OnItemsChanged(s, e);
        };
        Items = items;
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IStyle? ItemContainerStyle
    {
        get => GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public Collection<object> Items { get; }

    internal Panel? ItemsHost { get; set; }

    private bool _hasItems;

    internal bool HasItems
    {
        get => _hasItems;
        set
        {
            _hasItems = value;
            PseudoClasses.Set(":hasitems", value);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemTemplateProperty || change.Property == ItemContainerStyleProperty)
        {
            Refresh();
        }
        else if (change.Property == ItemsSourceProperty)
        {
            OnItemsSourceChanged((IEnumerable?)change.OldValue, (IEnumerable?)change.NewValue);
        }
    }

    protected virtual void OnItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue)
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ItemsHost?.Children.Clear();
        base.OnApplyTemplate(e);
        ItemsHost = e.NameScope.Find<Panel>(ElementPanel);
        Refresh();
    }

    protected virtual void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Refresh();
        UpdateItems();
    }

    protected virtual Control GetContainerForItemOverride() => new ContentControl();

    protected virtual bool IsItemItsOwnContainerOverride(object item) => item is ContentControl;

    protected virtual void PrepareContainerForItemOverride(Control element, object item)
    {
        if (element is ContentControl contentControl)
        {
            contentControl.Content = item;
            contentControl.ContentTemplate = ItemTemplate;
        }
    }

    void IAddChild.AddChild(object child)
    {
        if (child is not AvaloniaObject obj) return;
        Items.Add(obj);
    }

    protected virtual void Refresh()
    {
        if (ItemsHost == null) return;

        ItemsHost.Children.Clear();
        foreach (var item in Items)
        {
            Control container;
            if (IsItemItsOwnContainerOverride(item))
            {
                if (item is not Control control) continue;
                container = control;
            }
            else
            {
                container = GetContainerForItemOverride();
                PrepareContainerForItemOverride(container, item);
            }

            if (ItemContainerStyle != null)
            {
                container.Styles.Add(ItemContainerStyle);
            }
            ItemsHost.Children.Add(container);
        }
    }

    protected virtual void UpdateItems()
    {
    }
}