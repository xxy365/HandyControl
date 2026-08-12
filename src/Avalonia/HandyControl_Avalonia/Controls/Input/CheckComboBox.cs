using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace HandyControl.Controls;

[TemplatePart(PanelName, typeof(Panel))]
[TemplatePart(SelectAllName, typeof(CheckComboBoxItem))]
public class CheckComboBox : ListBox
{
    public const string PanelName = "PART_Panel";

    public const string SelectAllName = "PART_SelectAll";

    private Panel? _panel;

    private CheckComboBoxItem? _selectAllItem;

    private bool _isInternalAction;

    private readonly Dictionary<Tag, object?> _tagItemMap = new();

    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        AvaloniaProperty.Register<CheckComboBox, double>(nameof(MaxDropDownHeight), 400);

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<CheckComboBox, bool>(nameof(IsDropDownOpen), coerce: CoerceIsDropDownOpen);

    public static readonly StyledProperty<ControlTheme?> TagStyleProperty =
        AvaloniaProperty.Register<CheckComboBox, ControlTheme?>(nameof(TagStyle));

    public static readonly StyledProperty<double> TagSpacingProperty =
        AvaloniaProperty.Register<CheckComboBox, double>(nameof(TagSpacing), 3);

    public static readonly StyledProperty<bool> ShowSelectAllButtonProperty =
        AvaloniaProperty.Register<CheckComboBox, bool>(nameof(ShowSelectAllButton));

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<CheckComboBox, bool>(nameof(IsReadOnly), coerce: CoerceIsReadOnly);

    public static readonly StyledProperty<Avalonia.Layout.HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<CheckComboBox>();

    public static readonly StyledProperty<Avalonia.Layout.VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<CheckComboBox>();

    public double MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public ControlTheme? TagStyle
    {
        get => GetValue(TagStyleProperty);
        set => SetValue(TagStyleProperty, value);
    }

    public double TagSpacing
    {
        get => GetValue(TagSpacingProperty);
        set => SetValue(TagSpacingProperty, value);
    }

    public bool ShowSelectAllButton
    {
        get => GetValue(ShowSelectAllButtonProperty);
        set => SetValue(ShowSelectAllButtonProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public Avalonia.Layout.HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public Avalonia.Layout.VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    private static bool CoerceIsDropDownOpen(AvaloniaObject d, bool baseValue) =>
        ((CheckComboBox) d).IsReadOnly ? false : baseValue;

    private static bool CoerceIsReadOnly(AvaloniaObject d, bool baseValue)
    {
        if (baseValue)
        {
            ((CheckComboBox) d).IsDropDownOpen = false;
        }

        return baseValue;
    }

    public CheckComboBox()
    {
        SelectionMode = SelectionMode.Multiple;
        AddHandler(HandyControl.Controls.Tag.ClosedEvent, Tags_OnClosed);
        SelectionChanged += (_, _) => UpdateTags();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_selectAllItem != null)
        {
            _selectAllItem.PropertyChanged -= SelectAllItem_OnPropertyChanged;
        }

        base.OnApplyTemplate(e);

        _panel = e.NameScope.Find<Panel>(PanelName);
        _selectAllItem = e.NameScope.Find<CheckComboBoxItem>(SelectAllName);
        if (_selectAllItem != null)
        {
            _selectAllItem.PropertyChanged += SelectAllItem_OnPropertyChanged;
        }

        UpdateTags();
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) =>
        new CheckComboBoxItem();

    private void SelectAllItem_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ListBoxItem.IsSelectedProperty) return;
        SwitchAllItems(e.GetNewValue<bool>());
    }

    private void Tags_OnClosed(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Tag tag) return;

        if (_tagItemMap.TryGetValue(tag, out var item))
        {
            SelectedItems?.Remove(item);
        }

        _tagItemMap.Remove(tag);
        _panel?.Children.Remove(tag);
    }

    private void SwitchAllItems(bool selected)
    {
        if (_isInternalAction) return;
        _isInternalAction = true;

        if (selected)
        {
            Selection.SelectAll();
        }
        else
        {
            Selection.Clear();
        }

        _isInternalAction = false;
        UpdateTags();
    }

    private void UpdateTags()
    {
        if (_panel == null || _isInternalAction) return;

        if (_selectAllItem != null)
        {
            _isInternalAction = true;
            _selectAllItem.SetCurrentValue(ListBoxItem.IsSelectedProperty,
                Items.Count > 0 && SelectedItems?.Count == Items.Count);
            _isInternalAction = false;
        }

        _panel.Children.Clear();
        _tagItemMap.Clear();
        var tagStyle = TagStyle;
        var isReadOnly = IsReadOnly;
        var selectedItems = SelectedItems;

        if (selectedItems == null) return;

        foreach (var item in selectedItems)
        {
            var tag = new Tag
            {
                Content = item,
                ShowCloseButton = !isReadOnly
            };
            if (tagStyle != null)
            {
                tag.Theme = tagStyle;
            }

            _tagItemMap[tag] = item;
            _panel.Children.Add(tag);
        }
    }
}
