using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace HandyControl.Controls;

/// <summary>
///     抽屉容器
/// </summary>
[TemplatePart(ElementOverlay, typeof(Panel))]
public class DrawerContainer : ContentControl
{
    private const string ElementOverlay = "PART_Overlay";

    private Panel? _overlayPanel;

    internal Panel? OverlayPanel => _overlayPanel;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _overlayPanel = e.NameScope.Find<Panel>(ElementOverlay);
    }
}
