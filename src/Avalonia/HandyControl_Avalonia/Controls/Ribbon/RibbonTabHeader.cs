using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace HandyControl.Controls
{
    public class RibbonTabHeader : ContentControl
    {
        public Ribbon? Ribbon => Ribbon.GetRibbon(this);

        internal RibbonTab? RibbonTab
        {
            get
            {
                var itemsControl = ItemsControl.ItemsControlFromItemContainer(this);
                var ribbon = Ribbon;
                if (itemsControl == null || ribbon == null)
                {
                    return null;
                }

                var index = itemsControl.IndexFromContainer(this);
                return ribbon.ContainerFromIndex(index) as RibbonTab;
            }
        }

        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<RibbonTabHeader, bool>(nameof(IsSelected));

        public bool IsSelected
        {
            get => (bool) GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        internal void PrepareRibbonTabHeader()
        {
            var ribbonTab = RibbonTab;
            if (ribbonTab == null)
            {
                return;
            }

            this[!IsSelectedProperty] = ribbonTab[!RibbonTab.IsSelectedProperty];
            this[!IsVisibleProperty] = ribbonTab[!IsVisibleProperty];
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            PrepareRibbonTabHeader();
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            var ribbon = Ribbon;
            if (ribbon != null)
            {
                ribbon.NotifyMouseClickedOnTabHeader(this, e);
                e.Handled = true;
            }

            base.OnPointerPressed(e);
        }

        protected override void OnGotFocus(FocusChangedEventArgs e)
        {
            base.OnGotFocus(e);

            var ribbonTab = RibbonTab;
            if (ribbonTab == null)
            {
                return;
            }

            ribbonTab.IsSelected = true;
        }
    }
}
