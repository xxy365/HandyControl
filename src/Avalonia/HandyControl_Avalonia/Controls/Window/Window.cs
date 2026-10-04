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

        /// <summary>标题栏默认高度，与主题中 ExtendClientAreaTitleBarHeightHint 保持一致。</summary>
        private const double DefaultTitleBarHeight = 29d;

        /// <summary>最大化时标题栏额外补偿的高度，与 WPF 版 NonClientArea 行为保持一致。</summary>
        private const double MaximizedTitleBarHeightCompensation = 8d;

        private Control? _customTitleBar;
        private Button? _buttonMin;
        private Button? _buttonMax;
        private Button? _buttonRestore;
        private Button? _buttonClose;

        private bool _showCustomTitleBar = true;
        private bool _isFullScreen;

        /// <summary>未被最大化补偿/隐藏逻辑污染的基础标题栏高度。</summary>
        private double _baseTitleBarHeight;

        /// <summary>防止 <see cref="SyncTitleBarHeight"/> 与用户赋值之间互相触发。</summary>
        private bool _syncingTitleBarHeight;

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
            AvaloniaProperty.Register<Window, double>(nameof(CustomTitleBarHeight), DefaultTitleBarHeight);

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
            CustomTitleBarHeightProperty.Changed.AddClassHandler<Window>(OnCustomTitleBarHeightChanged);
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

            // 首次套用模板时以当前值作为基础高度（此时可能已被主题 Setter 覆盖为 29）
            if (CustomTitleBarHeight > 0)
                _baseTitleBarHeight = CustomTitleBarHeight;

            _isFullScreen = IsFullScreen;
            _showCustomTitleBar = ShowCustomTitleBar;
            SyncTitleBarHeight();
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

        /// <summary>
        /// 标题栏按钮高度同步。
        /// 不能只依赖主题里对 CustomTitleBarHeight 的绑定：控件 IsVisible=false 时
        /// Avalonia 会暂停该控件的布局与绑定求值，导致隐藏的按钮保留旧高度，
        /// 还原时会短暂错位。
        /// </summary>
        private void SyncCaptionButtonHeight()
        {
            var height = CustomTitleBarHeight;

            if (_buttonMin != null) _buttonMin.SetCurrentValue(Layoutable.HeightProperty, height);
            if (_buttonMax != null) _buttonMax.SetCurrentValue(Layoutable.HeightProperty, height);
            if (_buttonRestore != null) _buttonRestore.SetCurrentValue(Layoutable.HeightProperty, height);
            if (_buttonClose != null) _buttonClose.SetCurrentValue(Layoutable.HeightProperty, height);
        }

        private static void OnWindowStateChanged(Window w, AvaloniaPropertyChangedEventArgs e)
        {
            w.UpdateChromeButtons();

            // Avalonia 12 已修复 ExtendClientAreaToDecorationsHint 在最大化时的边框问题，
            // 迁移文档明确要求移除「最大化时增删 BorderThickness」这类早期变通做法。
            w.SyncTitleBarHeight();
        }

        private static void OnShowCustomTitleBarChanged(Window w, AvaloniaPropertyChangedEventArgs e)
        {
            w.SwitchShowCustomTitleBar((bool)(e.NewValue ?? false));
        }

        private static void OnIsFullScreenChanged(Window w, AvaloniaPropertyChangedEventArgs e)
        {
            w.SwitchIsFullScreen((bool)(e.NewValue ?? false));
        }

        private static void OnCustomTitleBarHeightChanged(Window w, AvaloniaPropertyChangedEventArgs e)
        {
            // SyncTitleBarHeight 内部的回写不视为用户改动，避免递归
            if (w._syncingTitleBarHeight) return;

            w._baseTitleBarHeight = Math.Max(0d, e.NewValue is double d ? d : 0d);
            w.SyncTitleBarHeight();
        }

        private void SwitchShowCustomTitleBar(bool showCustomTitleBar)
        {
            _showCustomTitleBar = showCustomTitleBar;
            SyncTitleBarHeight();
        }

        private void SwitchIsFullScreen(bool isFullScreen)
        {
            if (_isFullScreen == isFullScreen) return;
            _isFullScreen = isFullScreen;

            if (isFullScreen)
            {
                if (WindowState != WindowState.FullScreen)
                {
                    _tempWindowState = WindowState;
                    WindowState = WindowState.FullScreen;
                }
            }
            else if (WindowState == WindowState.FullScreen)
            {
                WindowState = _tempWindowState == WindowState.FullScreen ? WindowState.Normal : _tempWindowState;
            }

            SyncTitleBarHeight();
        }

        /// <summary>
        /// 依据当前窗口状态推导标题栏高度，并同步到模板与原生拖拽区。
        /// 原实现里最大化补偿与隐藏逻辑各自缓存一份高度会互相覆盖，
        /// 且 <see cref="CustomTitleBarHeight"/> 从未绑定到模板，导致属性形同虚设。
        /// </summary>
        private void SyncTitleBarHeight()
        {
            var height = !_showCustomTitleBar || _isFullScreen
                ? 0d
                : WindowState == WindowState.Maximized
                    ? _baseTitleBarHeight + MaximizedTitleBarHeightCompensation
                    : _baseTitleBarHeight;

            _syncingTitleBarHeight = true;
            try
            {
                SetCurrentValue(CustomTitleBarHeightProperty, height);
                SetCurrentValue(Avalonia.Controls.Window.ExtendClientAreaTitleBarHeightHintProperty, height);
            }
            finally
            {
                _syncingTitleBarHeight = false;
            }

            if (_customTitleBar != null)
                _customTitleBar.IsVisible = height > 0;

            SyncCaptionButtonHeight();
        }

        #endregion
    }
}