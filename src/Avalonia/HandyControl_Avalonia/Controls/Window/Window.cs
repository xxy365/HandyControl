using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace HandyControl.Controls
{
    /// <summary>
    /// �ޱ߿��Զ��崰�ڣ��ȼ� WPF HandyControl �� Window / WindowChrome Win10 ��񣩡�
    /// </summary>
    public class Window : Avalonia.Controls.Window
    {
        protected override Type StyleKeyOverride => typeof(Window);

        private const string ElementCustomTitleBar = "PART_CustomTitleBar";

        private Control? _customTitleBar;
        private Button? _buttonMin;
        private Button? _buttonMax;
        private Button? _buttonRestore;
        private Button? _buttonClose;

        private bool _showCustomTitleBar = true;
        private bool _isFullScreen;
        private double _tempCustomTitleBarHeight;
        private Thickness _actualBorderThickness;
        private WindowState _tempWindowState;

        #region props

        public static readonly StyledProperty<object?> CustomTitleBarContentProperty =
            AvaloniaProperty.Register<Window, object?>(nameof(CustomTitleBarContent));

        public object? CustomTitleBarContent
        {
            get => GetValue(CustomTitleBarContentProperty);
            set => SetValue(CustomTitleBarContentProperty, value);
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

        public static readonly StyledProperty<IBrush?> CustomTitleBarBackgroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(CustomTitleBarBackground));

        public IBrush? CustomTitleBarBackground
        {
            get => GetValue(CustomTitleBarBackgroundProperty);
            set => SetValue(CustomTitleBarBackgroundProperty, value);
        }

        public static readonly StyledProperty<IBrush?> CustomTitleBarForegroundProperty =
            AvaloniaProperty.Register<Window, IBrush?>(nameof(CustomTitleBarForeground));

        public IBrush? CustomTitleBarForeground
        {
            get => GetValue(CustomTitleBarForegroundProperty);
            set => SetValue(CustomTitleBarForegroundProperty, value);
        }

        public static readonly StyledProperty<double> CustomTitleBarHeightProperty =
            AvaloniaProperty.Register<Window, double>(nameof(CustomTitleBarHeight), 22.0);

        public double CustomTitleBarHeight
        {
            get => GetValue(CustomTitleBarHeightProperty);
            set => SetValue(CustomTitleBarHeightProperty, value);
        }

        public static readonly StyledProperty<bool> ShowCustomTitleBarProperty =
            AvaloniaProperty.Register<Window, bool>(nameof(ShowCustomTitleBar), true);

        public bool ShowCustomTitleBar
        {
            get => GetValue(ShowCustomTitleBarProperty);
            set => SetValue(ShowCustomTitleBarProperty, value);
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
        }

        static Window()
        {
            IsFullScreenProperty.Changed.AddClassHandler<Window>(OnIsFullScreenChanged);
            ShowCustomTitleBarProperty.Changed.AddClassHandler<Window>(OnShowCustomTitleBarChanged);
            WindowStateProperty.Changed.AddClassHandler<Window>(OnWindowStateChanged);
        }

        #region methods

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _customTitleBar = e.NameScope.Find<Control>(ElementCustomTitleBar);

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

            _tempCustomTitleBarHeight = CustomTitleBarHeight;

            SwitchIsFullScreen(IsFullScreen);
            SwitchShowCustomTitleBar(ShowCustomTitleBar);
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
                w._tempCustomTitleBarHeight = w.CustomTitleBarHeight;
                w.CustomTitleBarHeight += 8;
            }
            else if (newState != WindowState.Maximized && oldState == WindowState.Maximized)
            {
                w.BorderThickness = w._actualBorderThickness;
                w.CustomTitleBarHeight = w._tempCustomTitleBarHeight;
            }
        }

        private static void OnShowCustomTitleBarChanged(Window w, AvaloniaPropertyChangedEventArgs e)
        {
            w.SwitchShowCustomTitleBar((bool)(e.NewValue ?? false));
        }

        private static void OnIsFullScreenChanged(Window w, AvaloniaPropertyChangedEventArgs e)
        {
            w.SwitchIsFullScreen((bool)(e.NewValue ?? false));
        }

        private void SwitchShowCustomTitleBar(bool showCustomTitleBar)
        {
            _showCustomTitleBar = showCustomTitleBar;
            SetCustomTitleBarVisible(showCustomTitleBar && !_isFullScreen);
        }

        private void SwitchIsFullScreen(bool isFullScreen)
        {
            if (_isFullScreen == isFullScreen) return;
            _isFullScreen = isFullScreen;

            if (isFullScreen)
            {
                _tempCustomTitleBarHeight = CustomTitleBarHeight;
                if (_customTitleBar != null) _customTitleBar.IsVisible = false;
                CustomTitleBarHeight = 0;

                _tempWindowState = WindowState;
                WindowState = WindowState.FullScreen;
            }
            else
            {
                SetCustomTitleBarVisible(ShowCustomTitleBar);
                CustomTitleBarHeight = _tempCustomTitleBarHeight;

                WindowState = _tempWindowState;
            }
        }

        private void SetCustomTitleBarVisible(bool visible)
        {
            if (_customTitleBar != null) _customTitleBar.IsVisible = visible;
            _tempCustomTitleBarHeight = CustomTitleBarHeight;
            CustomTitleBarHeight = visible ? _tempCustomTitleBarHeight : 0;
        }

        #endregion
    }
}