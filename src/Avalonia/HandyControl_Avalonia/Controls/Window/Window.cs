using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace HandyControl.Controls
{
    /// <summary>
    /// 无边框自定义窗口（等价 WPF HandyControl 的 Window / WindowChrome Win10 风格）。
    /// </summary>
    public class Window : Avalonia.Controls.Window
    {
        private const string ElementNonClientArea = "PART_NonClientArea";

        private Control? _nonClientArea;
        private Button? _buttonMin;
        private Button? _buttonMax;
        private Button? _buttonRestore;
        private Button? _buttonClose;

        private bool _showNonClientArea = true;
        private bool _isFullScreen;
        private double _tempNonClientAreaHeight;
        private Thickness _actualBorderThickness;
        private WindowState _tempWindowState;

        #region props

        public static readonly StyledProperty<object?> NonClientAreaContentProperty =
            AvaloniaProperty.Register<Window, object?>(nameof(NonClientAreaContent));

        public object? NonClientAreaContent
        {
            get => GetValue(NonClientAreaContentProperty);
            set => SetValue(NonClientAreaContentProperty, value);
        }

        public static readonly StyledProperty<IBrush?> CloseButtonHoverBackgroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(CloseButtonHoverBackground));

        public IBrush? CloseButtonHoverBackground
        {
            get => GetValue(CloseButtonHoverBackgroundProperty);
            set => SetValue(CloseButtonHoverBackgroundProperty, value);
        }

        public static readonly StyledProperty<IBrush?> CloseButtonHoverForegroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(CloseButtonHoverForeground));

        public IBrush? CloseButtonHoverForeground
        {
            get => GetValue(CloseButtonHoverForegroundProperty);
            set => SetValue(CloseButtonHoverForegroundProperty, value);
        }

        public static readonly StyledProperty<IBrush?> CloseButtonBackgroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(CloseButtonBackground), Brushes.Transparent);

        public IBrush? CloseButtonBackground
        {
            get => GetValue(CloseButtonBackgroundProperty);
            set => SetValue(CloseButtonBackgroundProperty, value);
        }

        public static readonly StyledProperty<IBrush?> CloseButtonForegroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(CloseButtonForeground), Brushes.White);

        public IBrush? CloseButtonForeground
        {
            get => GetValue(CloseButtonForegroundProperty);
            set => SetValue(CloseButtonForegroundProperty, value);
        }

        public static readonly StyledProperty<IBrush?> OtherButtonBackgroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(OtherButtonBackground), Brushes.Transparent);

        public IBrush? OtherButtonBackground
        {
            get => GetValue(OtherButtonBackgroundProperty);
            set => SetValue(OtherButtonBackgroundProperty, value);
        }

        public static readonly StyledProperty<IBrush?> OtherButtonForegroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(OtherButtonForeground), Brushes.White);

        public IBrush? OtherButtonForeground
        {
            get => GetValue(OtherButtonForegroundProperty);
            set => SetValue(OtherButtonForegroundProperty, value);
        }

        public static readonly StyledProperty<IBrush?> OtherButtonHoverBackgroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(OtherButtonHoverBackground));

        public IBrush? OtherButtonHoverBackground
        {
            get => GetValue(OtherButtonHoverBackgroundProperty);
            set => SetValue(OtherButtonHoverBackgroundProperty, value);
        }

        public static readonly StyledProperty<IBrush?> OtherButtonHoverForegroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(OtherButtonHoverForeground));

        public IBrush? OtherButtonHoverForeground
        {
            get => GetValue(OtherButtonHoverForegroundProperty);
            set => SetValue(OtherButtonHoverForegroundProperty, value);
        }

        public static readonly StyledProperty<IBrush?> NonClientAreaBackgroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(NonClientAreaBackground));

        public IBrush? NonClientAreaBackground
        {
            get => GetValue(NonClientAreaBackgroundProperty);
            set => SetValue(NonClientAreaBackgroundProperty, value);
        }

        public static readonly StyledProperty<IBrush?> NonClientAreaForegroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(NonClientAreaForeground));

        public IBrush? NonClientAreaForeground
        {
            get => GetValue(NonClientAreaForegroundProperty);
            set => SetValue(NonClientAreaForegroundProperty, value);
        }

        public static readonly StyledProperty<double> NonClientAreaHeightProperty =
            AvaloniaProperty.Register<Window, double>(nameof(NonClientAreaHeight), 22.0);

        public double NonClientAreaHeight
        {
            get => GetValue(NonClientAreaHeightProperty);
            set => SetValue(NonClientAreaHeightProperty, value);
        }

        public static readonly StyledProperty<bool> ShowNonClientAreaProperty =
            AvaloniaProperty.Register<Window, bool>(nameof(ShowNonClientArea), true);

        public bool ShowNonClientArea
        {
            get => GetValue(ShowNonClientAreaProperty);
            set => SetValue(ShowNonClientAreaProperty, value);
        }

        public static readonly StyledProperty<bool> ShowTitleProperty =
            AvaloniaProperty.Register<Window, bool>(nameof(ShowTitle), true);

        public bool ShowTitle
        {
            get => GetValue(ShowTitleProperty);
            set => SetValue(ShowTitleProperty, value);
        }

        public static readonly StyledProperty<bool> ShowIconProperty =
            AvaloniaProperty.Register<Window, bool>(nameof(ShowIcon), true);

        public bool ShowIcon
        {
            get => GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        public static readonly StyledProperty<bool> IsFullScreenProperty =
            AvaloniaProperty.Register<Window, bool>(nameof(IsFullScreen), false);

        public bool IsFullScreen
        {
            get => GetValue(IsFullScreenProperty);
            set => SetValue(IsFullScreenProperty, value);
        }

        #endregion

        public Window()
        {
            WindowDecorations = WindowDecorations.None;
        }

        static Window()
        {
            IsFullScreenProperty.Changed.AddClassHandler<Window>(OnIsFullScreenChanged);
            ShowNonClientAreaProperty.Changed.AddClassHandler<Window>(OnShowNonClientAreaChanged);
            WindowStateProperty.Changed.AddClassHandler<Window>(OnWindowStateChanged);
        }

        #region methods

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_nonClientArea != null)
            {
                _nonClientArea.PointerPressed -= NonClientArea_OnPointerPressed;
                _nonClientArea.DoubleTapped -= NonClientArea_OnDoubleTapped;
            }

            _nonClientArea = e.NameScope.Find<Control>(ElementNonClientArea);
            if (_nonClientArea != null)
            {
                _nonClientArea.PointerPressed += NonClientArea_OnPointerPressed;
                _nonClientArea.DoubleTapped += NonClientArea_OnDoubleTapped;
            }

            if (_buttonMin != null) _buttonMin.Click -= ButtonMin_OnClick;
            if (_buttonMax != null) _buttonMax.Click -= ButtonMax_OnClick;
            if (_buttonRestore != null) _buttonRestore.Click -= ButtonRestore_OnClick;
            if (_buttonClose != null) _buttonClose.Click -= ButtonClose_OnClick;

            _buttonMin = e.NameScope.Find<Button>("ButtonMin");
            _buttonMax = e.NameScope.Find<Button>("ButtonMax");
            _buttonRestore = e.NameScope.Find<Button>("ButtonRestore");
            _buttonClose = e.NameScope.Find<Button>("ButtonClose");

            if (_buttonMin != null) _buttonMin.Click += ButtonMin_OnClick;
            if (_buttonMax != null) _buttonMax.Click += ButtonMax_OnClick;
            if (_buttonRestore != null) _buttonRestore.Click += ButtonRestore_OnClick;
            if (_buttonClose != null) _buttonClose.Click += ButtonClose_OnClick;

            UpdateChromeButtons();

            _tempNonClientAreaHeight = NonClientAreaHeight;

            SwitchIsFullScreen(IsFullScreen);
            SwitchShowNonClientArea(ShowNonClientArea);
        }

        private void NonClientArea_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(_nonClientArea).Properties.IsLeftButtonPressed && e.Source is not Button)
            {
                BeginMoveDrag(e);
            }
        }

        private void NonClientArea_OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (e.Source is Button) return;

            if (CanResize)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        }

        private void ButtonMin_OnClick(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ButtonMax_OnClick(object? sender, RoutedEventArgs e) => WindowState = WindowState.Maximized;

        private void ButtonRestore_OnClick(object? sender, RoutedEventArgs e) => WindowState = WindowState.Normal;

        private void ButtonClose_OnClick(object? sender, RoutedEventArgs e) => Close();

        private void UpdateChromeButtons()
        {
            var maximized = WindowState == WindowState.Maximized;
            var fullScreen = WindowState == WindowState.FullScreen;

            if (_buttonMin != null) _buttonMin.IsVisible = !fullScreen;
            if (_buttonMax != null) _buttonMax.IsVisible = !maximized && !fullScreen;
            if (_buttonRestore != null) _buttonRestore.IsVisible = maximized && !fullScreen;
            if (_buttonClose != null) _buttonClose.IsVisible = !fullScreen;
        }

        private static void OnWindowStateChanged(Window w, AvaloniaPropertyChangedEventArgs e)
        {
            w.UpdateChromeButtons();

            var oldState = e.OldValue is WindowState oldVal ? oldVal : WindowState.Normal;
            var newState = e.NewValue is WindowState newVal ? newVal : WindowState.Normal;

            if (newState == WindowState.Maximized && oldState != WindowState.Maximized)
            {
                w._actualBorderThickness = w.BorderThickness;
                w.BorderThickness = new Thickness();
                w._tempNonClientAreaHeight = w.NonClientAreaHeight;
                w.NonClientAreaHeight += 8;
            }
            else if (newState != WindowState.Maximized && oldState == WindowState.Maximized)
            {
                w.BorderThickness = w._actualBorderThickness;
                w.NonClientAreaHeight = w._tempNonClientAreaHeight;
            }
        }

        private static void OnShowNonClientAreaChanged(Window w, AvaloniaPropertyChangedEventArgs e)
        {
            w.SwitchShowNonClientArea((bool)(e.NewValue ?? false));
        }

        private static void OnIsFullScreenChanged(Window w, AvaloniaPropertyChangedEventArgs e)
        {
            w.SwitchIsFullScreen((bool)(e.NewValue ?? false));
        }

        private void SwitchShowNonClientArea(bool showNonClientArea)
        {
            _showNonClientArea = showNonClientArea;
            SetNonClientAreaVisible(showNonClientArea && !_isFullScreen);
        }

        private void SwitchIsFullScreen(bool isFullScreen)
        {
            if (_isFullScreen == isFullScreen) return;
            _isFullScreen = isFullScreen;

            if (isFullScreen)
            {
                _tempNonClientAreaHeight = NonClientAreaHeight;
                if (_nonClientArea != null) _nonClientArea.IsVisible = false;
                NonClientAreaHeight = 0;

                _tempWindowState = WindowState;
                WindowState = WindowState.FullScreen;
            }
            else
            {
                SetNonClientAreaVisible(ShowNonClientArea);
                NonClientAreaHeight = _tempNonClientAreaHeight;

                WindowState = _tempWindowState;
            }
        }

        private void SetNonClientAreaVisible(bool visible)
        {
            if (_nonClientArea != null) _nonClientArea.IsVisible = visible;
            _tempNonClientAreaHeight = NonClientAreaHeight;
            NonClientAreaHeight = visible ? _tempNonClientAreaHeight : 0;
        }

        #endregion
    }
}