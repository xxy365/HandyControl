using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using HandyControl.Data;
using HandyControl.Properties.Langs;
using HandyControl.Tools;

namespace HandyControl.Controls
{
    /// <summary>
    /// 弹出窗口（无系统标题栏，标题条可拖动，含确认/取消/关闭按钮）。
    /// </summary>
    public class PopupWindow : Avalonia.Controls.Window
    {
        private const string ElementMainBorder = "PART_MainBorder";
        private const string ElementTitleBlock = "PART_TitleBlock";

        public static readonly StyledProperty<string?> ContentStrProperty =
            AvaloniaProperty.Register<PopupWindow, string?>(nameof(ContentStr));

        public string? ContentStr
        {
            get => GetValue(ContentStrProperty);
            internal set => SetValue(ContentStrProperty, value);
        }

        public static readonly StyledProperty<bool> ShowTitleProperty =
            AvaloniaProperty.Register<PopupWindow, bool>(nameof(ShowTitle), true);

        public bool ShowTitle
        {
            get => GetValue(ShowTitleProperty);
            set => SetValue(ShowTitleProperty, value);
        }

        public static readonly StyledProperty<bool> ShowCancelProperty =
            AvaloniaProperty.Register<PopupWindow, bool>(nameof(ShowCancel), false);

        public bool ShowCancel
        {
            get => GetValue(ShowCancelProperty);
            set => SetValue(ShowCancelProperty, value);
        }

        public static readonly StyledProperty<bool> ShowBorderProperty =
            AvaloniaProperty.Register<PopupWindow, bool>(nameof(ShowBorder), false);

        public bool ShowBorder
        {
            get => GetValue(ShowBorderProperty);
            set => SetValue(ShowBorderProperty, value);
        }

        private Border? _mainBorder;
        private TextBlock? _titleBlock;
        private bool _showBackground = true;
        private Control? _targetElement;
        private bool _isDialog;

        public Control? PopupElement { get; set; }

        public PopupWindow()
        {
            WindowDecorations = WindowDecorations.None;
            TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
            Background = Brushes.Transparent;
            CanResize = false;
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.WidthAndHeight;
            MinWidth = 300;
            MinHeight = 220;

            Opened += (_, _) =>
            {
                if (!_showBackground)
                {
                    PositionWindow();
                }
            };

            Closed += (_, _) => PopupElement = null;

            try
            {
                Owner = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;
            }
            catch
            {
                // ignored
            }
        }

        public PopupWindow(Avalonia.Controls.Window owner) : this()
        {
            Owner = owner;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_titleBlock != null)
            {
                _titleBlock.PointerPressed -= TitleBlock_OnPointerPressed;
            }

            _mainBorder = e.NameScope.Find<Border>(ElementMainBorder);
            _titleBlock = e.NameScope.Find<TextBlock>(ElementTitleBlock);

            if (_titleBlock != null)
            {
                _titleBlock.PointerPressed += TitleBlock_OnPointerPressed;
            }

            var buttonClose = e.NameScope.Find<Button>("ButtonClose");
            if (buttonClose != null)
            {
                buttonClose.Click += ButtonClose_OnClick;
            }

            var buttonOk = e.NameScope.Find<Button>("ButtonOk");
            if (buttonOk != null)
            {
                buttonOk.Click += ButtonOk_OnClick;
            }

            var buttonCancel = e.NameScope.Find<Button>("ButtonCancel");
            if (buttonCancel != null)
            {
                buttonCancel.Click += ButtonCancel_OnClick;
            }

            if (PopupElement != null && _mainBorder != null)
            {
                _mainBorder.Child = PopupElement;
            }
        }

        private void TitleBlock_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(_titleBlock).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void PositionWindow()
        {
            if (_targetElement == null)
            {
                Opacity = 1;
                return;
            }

            var screen = _targetElement.PointToScreen(new Point(0, 0));
            var size = _targetElement.Bounds.Size;
            Position = new PixelPoint(
                (int)(screen.X + size.Width / 2 - 150),
                (int)(screen.Y + size.Height / 2 - 110));
            Opacity = 1;
        }

        public void Show(Control element, bool showBackground = true)
        {
            if (!showBackground)
            {
                Opacity = 0;
                ShowTitle = false;
                MinWidth = 0;
                MinHeight = 0;
            }

            _showBackground = showBackground;
            _targetElement = element;
            Show();
        }

        public void ShowDialog(Control element, bool showBackground = true)
        {
            _isDialog = true;

            if (!showBackground)
            {
                Opacity = 0;
                ShowTitle = false;
                MinWidth = 0;
                MinHeight = 0;
            }

            _showBackground = showBackground;
            _targetElement = element;
            ShowDialogCore();
        }

        public async Task ShowDialogAsync(Control element, bool showBackground = true)
        {
            _isDialog = true;

            if (!showBackground)
            {
                Opacity = 0;
                ShowTitle = false;
                MinWidth = 0;
                MinHeight = 0;
            }

            _showBackground = showBackground;
            _targetElement = element;

            if (Owner is Avalonia.Controls.Window owner)
            {
                await base.ShowDialog(owner);
            }
            else
            {
                Show();
            }
        }

        public void Show(Avalonia.Controls.Window element, Point point)
        {
            Position = new PixelPoint((int)(element.Position.X + point.X), (int)(element.Position.Y + point.Y));
            Show();
        }

        private void ShowDialogCore()
        {
            if (Owner is Avalonia.Controls.Window owner)
            {
                var signal = new System.Threading.ManualResetEventSlim(false);
                Closed += (_, _) => signal.Set();
                base.ShowDialog(owner);
                var dispatcher = Dispatcher.UIThread;
                while (!signal.IsSet)
                {
                    dispatcher.RunJobs();
                }
            }
            else
            {
                Show();
            }
        }

        public static void Show(string message)
        {
            var window = new PopupWindow
            {
                ContentStr = message,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = ResourceHelper.GetResource<IBrush>(ResourceToken.PrimaryBrush)
            };
            window.Show();
        }

        public static bool? ShowDialog(string message, string? title = null, bool showCancel = false)
        {
            var window = new PopupWindow
            {
                ContentStr = message,
                _isDialog = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowBorder = true,
                Title = string.IsNullOrEmpty(title) ? Lang.Tip : title!,
                ShowCancel = showCancel
            };
            window.ShowDialogCore();
            return window._isDialog ? (bool?)true : null;
        }

        private void ButtonClose_OnClick(object? sender, RoutedEventArgs e) => Close();

        private void ButtonOk_OnClick(object? sender, RoutedEventArgs e) => Close();

        private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e) => Close();
    }
}