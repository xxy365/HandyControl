using Avalonia;
using HandyControl.Data;

namespace HandyControl.Controls;

/// <summary>
///     步骤条单元项
/// </summary>
public class StepBarItem : SelectableItem
{
    /// <summary>
    ///     步骤编号
    /// </summary>
    public static readonly StyledProperty<int> IndexProperty =
        AvaloniaProperty.Register<StepBarItem, int>(nameof(Index), -1);

    /// <summary>
    ///     步骤编号
    /// </summary>
    public int Index
    {
        get => GetValue(IndexProperty);
        internal set => SetValue(IndexProperty, value);
    }

    /// <summary>
    ///     步骤状态
    /// </summary>
    public static readonly StyledProperty<StepStatus> StatusProperty =
        AvaloniaProperty.Register<StepBarItem, StepStatus>(nameof(Status), StepStatus.Waiting);

    /// <summary>
    ///     步骤状态
    /// </summary>
    public StepStatus Status
    {
        get => GetValue(StatusProperty);
        internal set => SetValue(StatusProperty, value);
    }
}