using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace HandyControl.Controls;

/// <summary>
///     带上下文菜单的切换按钮
/// </summary>
public class ContextMenuToggleButton : ToggleButton
{
    public ContextMenu? Menu { get; set; }

    protected override void OnClick()
    {
        base.OnClick();
        if (Menu != null)
        {
            if (IsChecked == true)
            {
                Menu.PlacementTarget = this;
                Menu.Open(this);
            }
            else
            {
                Menu.Close();
            }
        }
    }
}