using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace HandyControl.Controls;

public class ScrollBarAttach
{
    public static readonly AttachedProperty<bool> KeepVisibleOnDragProperty =
        AvaloniaProperty.RegisterAttached<ScrollBarAttach, ScrollBar, bool>("KeepVisibleOnDrag");

    public static void SetKeepVisibleOnDrag(AvaloniaObject element, bool value) => element.SetValue(KeepVisibleOnDragProperty, value);

    public static bool GetKeepVisibleOnDrag(AvaloniaObject element) => element.GetValue<bool>(KeepVisibleOnDragProperty);

    static ScrollBarAttach()
    {
        KeepVisibleOnDragProperty.Changed.AddClassHandler<ScrollBar>(OnKeepVisibleOnDragChanged);
    }

    private static void OnKeepVisibleOnDragChanged(ScrollBar scrollBar, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<bool>())
        {
            scrollBar.TemplateApplied += OnTemplateApplied;
            TryAttach(scrollBar);
        }
        else
        {
            scrollBar.TemplateApplied -= OnTemplateApplied;
            Detach(scrollBar);
        }
    }

    private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (sender is ScrollBar scrollBar)
        {
            TryAttach(scrollBar);
        }
    }

    private static void TryAttach(ScrollBar scrollBar)
    {
        var thumb = scrollBar.GetVisualDescendants().OfType<Thumb>().FirstOrDefault();
        if (thumb is null)
        {
            return;
        }

        thumb.DragStarted -= OnDragStarted;
        thumb.DragCompleted -= OnDragCompleted;
        thumb.DragStarted += OnDragStarted;
        thumb.DragCompleted += OnDragCompleted;
    }

    private static void Detach(ScrollBar scrollBar)
    {
        foreach (var thumb in scrollBar.GetVisualDescendants().OfType<Thumb>())
        {
            thumb.DragStarted -= OnDragStarted;
            thumb.DragCompleted -= OnDragCompleted;
        }
    }

    private static void OnDragStarted(object? sender, VectorEventArgs e)
    {
        if (sender is Thumb thumb && thumb.TemplatedParent is ScrollBar scrollBar)
        {
            scrollBar.Opacity = 0.8;
        }
    }

    private static void OnDragCompleted(object? sender, VectorEventArgs e)
    {
        if (sender is Thumb thumb && thumb.TemplatedParent is ScrollBar scrollBar)
        {
            scrollBar.ClearValue(Visual.OpacityProperty);
        }
    }
}
