using Avalonia;
using Avalonia.Controls;

namespace HandyControl.Controls
{
    public class RibbonTabHeaderItemsControl : ItemsControl
    {
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is RibbonTabHeader)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new RibbonTabHeader();
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is RibbonTabHeader ribbonTabHeader)
        {
            ribbonTabHeader.PrepareRibbonTabHeader();
        }
    }
    }
}
