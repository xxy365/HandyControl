using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using HandyControl.Data;

namespace HandyControl.Controls;

[TemplatePart(PartProgressBarBack, typeof(ProgressBar))]
public class StepBar : ItemsControl
{
    public const string PartProgressBarBack = "PART_ProgressBarBack";

    private ProgressBar? _progressBarBack;
    private int _oriStepIndex = -1;

    static StepBar()
    {
        StepIndexProperty.Changed.AddClassHandler<StepBar>((s, e) => s.OnStepIndexChanged(e.GetNewValue<int>()));
    }

    public StepBar()
    {
        AddHandler(SelectableItem.SelectedEvent, new EventHandler<RoutedEventArgs>(OnStepBarItemSelected));
        ContainerPrepared += OnContainerPrepared;
    }

    private void OnStepBarItemSelected(object? sender, RoutedEventArgs e)
    {
        if (!IsMouseSelectable)
        {
            return;
        }

        if (e.Source is StepBarItem item)
        {
            SetCurrentValue(StepIndexProperty, item.Index - 1);
        }
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is StepBarItem stepBarItem)
        {
            stepBarItem.Index = e.Index + 1;
        }

        var count = Items.Count;
        if (_progressBarBack != null)
        {
            _progressBarBack.Maximum = count - 1;
            _progressBarBack.Value = StepIndex;
        }

        if (count <= 0)
        {
            return;
        }

        if (_oriStepIndex > 0)
        {
            StepIndex = _oriStepIndex;
            _oriStepIndex = -1;
        }
        else
        {
            OnStepIndexChanged(StepIndex);
        }
    }

    private static int CoerceStepIndex(AvaloniaObject d, int baseValue)
    {
        var ctl = (StepBar)d;
        var stepIndex = baseValue;
        if (ctl.Items.Count == 0 && stepIndex > 0)
        {
            ctl._oriStepIndex = stepIndex;
            return 0;
        }

        return stepIndex < 0
            ? 0
            : stepIndex >= ctl.Items.Count
                ? ctl.Items.Count == 0 ? 0 : ctl.Items.Count - 1
                : baseValue;
    }

    private void OnStepIndexChanged(int stepIndex)
    {
        for (var i = 0; i < stepIndex; i++)
        {
            if (ContainerFromIndex(i) is StepBarItem stepItemFinished)
            {
                stepItemFinished.Status = StepStatus.Complete;
            }
        }

        for (var i = stepIndex + 1; i < Items.Count; i++)
        {
            if (ContainerFromIndex(i) is StepBarItem stepItemWaiting)
            {
                stepItemWaiting.Status = StepStatus.Waiting;
            }
        }

        if (ContainerFromIndex(stepIndex) is StepBarItem stepItemSelected)
        {
            stepItemSelected.Status = StepStatus.UnderWay;
        }

        if (_progressBarBack != null)
        {
            _progressBarBack.Value = stepIndex;
        }

        RaiseEvent(new FunctionEventArgs<int>(StepChangedEvent, this)
        {
            Info = stepIndex
        });
    }

    /// <summary>
    ///     步骤改变事件
    /// </summary>
    public static readonly RoutedEvent<FunctionEventArgs<int>> StepChangedEvent =
        RoutedEvent.Register<StepBar, FunctionEventArgs<int>>(nameof(StepChanged), RoutingStrategies.Bubble);

    /// <summary>
    ///     步骤改变事件
    /// </summary>
    public event EventHandler<FunctionEventArgs<int>>? StepChanged
    {
        add => AddHandler(StepChangedEvent, value);
        remove => RemoveHandler(StepChangedEvent, value);
    }

    public static readonly StyledProperty<int> StepIndexProperty =
        AvaloniaProperty.Register<StepBar, int>(nameof(StepIndex), 0, coerce: CoerceStepIndex);

    public int StepIndex
    {
        get => GetValue(StepIndexProperty);
        set => SetValue(StepIndexProperty, value);
    }

    public static readonly StyledProperty<Dock> DockProperty =
        AvaloniaProperty.Register<StepBar, Dock>(nameof(Dock), Dock.Top);

    public Dock Dock
    {
        get => GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    public static readonly StyledProperty<bool> IsMouseSelectableProperty =
        AvaloniaProperty.Register<StepBar, bool>(nameof(IsMouseSelectable));

    public bool IsMouseSelectable
    {
        get => GetValue(IsMouseSelectableProperty);
        set => SetValue(IsMouseSelectableProperty, value);
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is StepBarItem)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = typeof(StepBarItem);
        return true;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        => new StepBarItem();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _progressBarBack = e.NameScope.Find<ProgressBar>(PartProgressBarBack);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);

        var colCount = Items.Count;
        if (_progressBarBack == null || colCount <= 0)
        {
            return size;
        }

        if (Dock is Dock.Top or Dock.Bottom)
        {
            _progressBarBack.Width = (colCount - 1) * (Bounds.Width / colCount);
        }
        else
        {
            _progressBarBack.Height = (colCount - 1) * (Bounds.Height / colCount);
        }

        return size;
    }

    public void Next() => StepIndex++;

    public void Prev() => StepIndex--;
}