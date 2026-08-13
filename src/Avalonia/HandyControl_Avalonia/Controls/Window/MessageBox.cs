using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using HandyControl.Data;
using HandyControl.Properties.Langs;
using HandyControl.Tools;
using HandyControl.Tools.Helper;

namespace HandyControl.Controls
{
    public sealed class MessageBox : Window
    {
        private const string ElementPanel = "PART_Panel";
        private const string ElementButtonClose = "PART_ButtonClose";

        public static readonly StyledProperty<string?> MessageProperty =
            AvaloniaProperty.Register<MessageBox, string?>(nameof(Message));

        public string? Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly StyledProperty<Geometry?> ImageProperty =
            AvaloniaProperty.Register<MessageBox, Geometry?>(nameof(Image));

        public Geometry? Image
        {
            get => GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public static readonly StyledProperty<IBrush?> ImageBrushProperty =
            AvaloniaProperty.Register<MessageBox, IBrush?>(nameof(ImageBrush));

        public IBrush? ImageBrush
        {
            get => GetValue(ImageBrushProperty);
            set => SetValue(ImageBrushProperty, value);
        }

        public static readonly StyledProperty<bool> ShowImageProperty =
            AvaloniaProperty.Register<MessageBox, bool>(nameof(ShowImage), false);

        public bool ShowImage
        {
            get => GetValue(ShowImageProperty);
            set => SetValue(ShowImageProperty, value);
        }

        private Button? _buttonClose;
        private Button? _buttonOk;
        private Button? _buttonCancel;
        private Button? _buttonYes;
        private Button? _buttonNo;
        private bool _showCancel;
        private MessageBoxResult _messageBoxResult = MessageBoxResult.Cancel;

        public MessageBoxResult MessageBoxResult => _messageBoxResult;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var panel = e.NameScope.Find<Panel>(ElementPanel);
            if (panel != null)
            {
                if (_buttonOk != null) panel.Children.Add(_buttonOk);
                if (_buttonYes != null) panel.Children.Add(_buttonYes);
                if (_buttonNo != null) panel.Children.Add(_buttonNo);
                if (_buttonCancel != null) panel.Children.Add(_buttonCancel);
            }

            _buttonClose = e.NameScope.Find<Button>(ElementButtonClose);
            if (_buttonClose != null)
            {
                _buttonClose.Click += ButtonClose_Click;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape && _showCancel)
            {
                _messageBoxResult = MessageBoxResult.Cancel;
                Close();
            }
        }

        private void ButtonClose_Click(object? sender, RoutedEventArgs e) => Close();

        private void HandleResult(MessageBoxResult result)
        {
            _messageBoxResult = result;
            Close();
        }

        public static MessageBoxResult Success(string messageBoxText, string? caption = null) =>
            ShowCore(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK,
                iconKey: ResourceToken.SuccessGeometry, iconBrushKey: ResourceToken.SuccessBrush);

        public static MessageBoxResult Info(string messageBoxText, string? caption = null) =>
            ShowCore(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK,
                iconKey: ResourceToken.InfoGeometry, iconBrushKey: ResourceToken.InfoBrush);

        public static MessageBoxResult Warning(string messageBoxText, string? caption = null) =>
            ShowCore(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK,
                iconKey: ResourceToken.WarningGeometry, iconBrushKey: ResourceToken.WarningBrush);

        public static MessageBoxResult Error(string messageBoxText, string? caption = null) =>
            ShowCore(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                iconKey: ResourceToken.ErrorGeometry, iconBrushKey: ResourceToken.DangerBrush);

        public static MessageBoxResult Fatal(string messageBoxText, string? caption = null) =>
            ShowCore(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK,
                iconKey: ResourceToken.FatalGeometry, iconBrushKey: ResourceToken.PrimaryTextBrush);

        public static MessageBoxResult Ask(string messageBoxText, string? caption = null) =>
            ShowCore(null, messageBoxText, caption, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel,
                iconKey: ResourceToken.AskGeometry, iconBrushKey: ResourceToken.AccentBrush);

        public static MessageBoxResult Show(string messageBoxText, string? caption = null,
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None)
        {
            return ShowCore(null, messageBoxText, caption, button, icon, defaultResult);
        }

        public static MessageBoxResult Show(Avalonia.Controls.Window? owner, string messageBoxText, string? caption = null,
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None)
        {
            return ShowCore(owner, messageBoxText, caption, button, icon, defaultResult);
        }

        public static MessageBoxResult Show(MessageBoxInfo info)
        {
            return ShowCore(info.Owner, info.Message ?? string.Empty, info.Caption, info.Button, MessageBoxImage.None,
                info.DefaultResult, info.IconKey, info.Icon, info.IconBrushKey, info.IconBrush, info.StyleKey, info.Style);
        }

        public static Task<MessageBoxResult> SuccessAsync(string messageBoxText, string? caption = null) =>
            ShowCoreAsync(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK,
                iconKey: ResourceToken.SuccessGeometry, iconBrushKey: ResourceToken.SuccessBrush);

        public static Task<MessageBoxResult> InfoAsync(string messageBoxText, string? caption = null) =>
            ShowCoreAsync(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK,
                iconKey: ResourceToken.InfoGeometry, iconBrushKey: ResourceToken.InfoBrush);

        public static Task<MessageBoxResult> WarningAsync(string messageBoxText, string? caption = null) =>
            ShowCoreAsync(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK,
                iconKey: ResourceToken.WarningGeometry, iconBrushKey: ResourceToken.WarningBrush);

        public static Task<MessageBoxResult> ErrorAsync(string messageBoxText, string? caption = null) =>
            ShowCoreAsync(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                iconKey: ResourceToken.ErrorGeometry, iconBrushKey: ResourceToken.DangerBrush);

        public static Task<MessageBoxResult> FatalAsync(string messageBoxText, string? caption = null) =>
            ShowCoreAsync(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK,
                iconKey: ResourceToken.FatalGeometry, iconBrushKey: ResourceToken.PrimaryTextBrush);

        public static Task<MessageBoxResult> AskAsync(string messageBoxText, string? caption = null) =>
            ShowCoreAsync(null, messageBoxText, caption, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel,
                iconKey: ResourceToken.AskGeometry, iconBrushKey: ResourceToken.AccentBrush);

        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string? caption = null,
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None)
        {
            return ShowCoreAsync(null, messageBoxText, caption, button, icon, defaultResult);
        }

        public static Task<MessageBoxResult> ShowAsync(Avalonia.Controls.Window? owner, string messageBoxText, string? caption = null,
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None)
        {
            return ShowCoreAsync(owner, messageBoxText, caption, button, icon, defaultResult);
        }

        public static Task<MessageBoxResult> ShowAsync(MessageBoxInfo info)
        {
            return ShowCoreAsync(info.Owner, info.Message ?? string.Empty, info.Caption, info.Button, MessageBoxImage.None,
                info.DefaultResult, info.IconKey, info.Icon, info.IconBrushKey, info.IconBrush, info.StyleKey, info.Style);
        }

        private static MessageBoxResult ShowCore(
            Avalonia.Controls.Window? owner,
            string messageBoxText,
            string? caption,
            MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult,
            string? iconKey = null,
            Geometry? iconGeometry = null,
            string? iconBrushKey = null,
            IBrush? iconBrush = null,
            string? styleKey = null,
            ControlTheme? style = null)
        {
            if (!IsValidMessageBoxButton(button))
            {
                throw new InvalidEnumArgumentException(nameof(button), (int)button, typeof(MessageBoxButton));
            }

            if (!IsValidMessageBoxImage(icon))
            {
                throw new InvalidEnumArgumentException(nameof(icon), (int)icon, typeof(MessageBoxImage));
            }

            if (!IsValidMessageBoxResult(defaultResult))
            {
                throw new InvalidEnumArgumentException(nameof(defaultResult), (int)defaultResult, typeof(MessageBoxResult));
            }

            MessageBox? messageBox = null;

            Dispatcher.UIThread.Invoke(() =>
            {
                var ownerWindow = owner ?? WindowHelper.GetActiveWindow();
                var ownerIsNull = ownerWindow is null;

                messageBox = new MessageBox
                {
                    Message = messageBoxText,
                    Owner = ownerWindow,
                    WindowStartupLocation = ownerIsNull ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
                    ShowTitle = true,
                    Title = caption ?? string.Empty,
                    Topmost = ownerIsNull,
                    _messageBoxResult = defaultResult
                };

                SetButtonStatus(messageBox, button, defaultResult);
                SetImage(messageBox, icon, iconKey, iconGeometry, iconBrushKey, iconBrush);

                if (!string.IsNullOrEmpty(styleKey))
                {
                    messageBox.Theme = ResourceHelper.GetResource<ControlTheme>(styleKey) ?? style;
                }

                ShowDialogBlocking(messageBox, ownerWindow);
            });

            return messageBox!._messageBoxResult;
        }

        private static async Task<MessageBoxResult> ShowCoreAsync(
            Avalonia.Controls.Window? owner,
            string messageBoxText,
            string? caption,
            MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult,
            string? iconKey = null,
            Geometry? iconGeometry = null,
            string? iconBrushKey = null,
            IBrush? iconBrush = null,
            string? styleKey = null,
            ControlTheme? style = null)
        {
            if (!IsValidMessageBoxButton(button))
            {
                throw new InvalidEnumArgumentException(nameof(button), (int)button, typeof(MessageBoxButton));
            }

            if (!IsValidMessageBoxImage(icon))
            {
                throw new InvalidEnumArgumentException(nameof(icon), (int)icon, typeof(MessageBoxImage));
            }

            if (!IsValidMessageBoxResult(defaultResult))
            {
                throw new InvalidEnumArgumentException(nameof(defaultResult), (int)defaultResult, typeof(MessageBoxResult));
            }

            MessageBox? messageBox = null;
            Avalonia.Controls.Window? ownerWindow = null;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ownerWindow = owner ?? WindowHelper.GetActiveWindow();
                var ownerIsNull = ownerWindow is null;

                messageBox = new MessageBox
                {
                    Message = messageBoxText,
                    Owner = ownerWindow,
                    WindowStartupLocation = ownerIsNull ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
                    ShowTitle = true,
                    Title = caption ?? string.Empty,
                    Topmost = ownerIsNull,
                    _messageBoxResult = defaultResult
                };

                SetButtonStatus(messageBox, button, defaultResult);
                SetImage(messageBox, icon, iconKey, iconGeometry, iconBrushKey, iconBrush);

                if (!string.IsNullOrEmpty(styleKey))
                {
                    messageBox.Theme = ResourceHelper.GetResource<ControlTheme>(styleKey) ?? style;
                }
            });

            if (ownerWindow == null)
            {
                messageBox!.Show();
                return messageBox.MessageBoxResult;
            }

            await messageBox!.ShowDialog(ownerWindow);
            return messageBox.MessageBoxResult;
        }

        private static void SetButtonStatus(MessageBox messageBox, MessageBoxButton messageBoxButton, MessageBoxResult defaultResult)
        {
            switch (messageBoxButton)
            {
                case MessageBoxButton.OK:
                    messageBox._buttonOk = CreateButton(messageBox, Lang.Confirm, MessageBoxResult.OK, defaultResult != MessageBoxResult.Cancel);
                    break;

                case MessageBoxButton.OKCancel:
                    messageBox._buttonOk = CreateButton(messageBox, Lang.Confirm, MessageBoxResult.OK, defaultResult != MessageBoxResult.Cancel);

                    messageBox._showCancel = true;
                    messageBox._buttonCancel = CreateButton(messageBox, Lang.Cancel, MessageBoxResult.Cancel, defaultResult == MessageBoxResult.Cancel);
                    break;

                case MessageBoxButton.YesNo:
                    messageBox._buttonYes = CreateButton(messageBox, Lang.Yes, MessageBoxResult.Yes, defaultResult != MessageBoxResult.No);

                    messageBox._buttonNo = CreateButton(messageBox, Lang.No, MessageBoxResult.No, defaultResult == MessageBoxResult.No);
                    break;

                case MessageBoxButton.YesNoCancel:
                    messageBox._buttonYes = CreateButton(messageBox, Lang.Yes, MessageBoxResult.Yes, defaultResult is not (MessageBoxResult.No or MessageBoxResult.Cancel));

                    messageBox._buttonNo = CreateButton(messageBox, Lang.No, MessageBoxResult.No, false);

                    messageBox._showCancel = true;
                    messageBox._buttonCancel = CreateButton(messageBox, Lang.Cancel, MessageBoxResult.Cancel, defaultResult == MessageBoxResult.Cancel);
                    break;
            }
        }

        private static Button CreateButton(MessageBox messageBox, object content, MessageBoxResult result, bool isPrimary)
        {
            var button = new Button
            {
                Content = content,
                MinWidth = 88,
                Margin = new Thickness(5, 0),
                Theme = ResourceHelper.GetResource<ControlTheme>(isPrimary ? "MessageBoxPrimaryButtonStyle" : "MessageBoxButtonStyle")
            };

            button.Click += (_, _) => messageBox.HandleResult(result);
            return button;
        }

        private static void SetImage(MessageBox messageBox, MessageBoxImage messageBoxImage,
            string? iconKey, Geometry? iconGeometry, string? iconBrushKey, IBrush? iconBrush)
        {
            if (string.IsNullOrEmpty(iconKey))
            {
                switch (messageBoxImage)
                {
                    case MessageBoxImage.Error:
                        iconKey = ResourceToken.ErrorGeometry;
                        iconBrushKey = ResourceToken.DangerBrush;
                        break;
                    case MessageBoxImage.Question:
                        iconKey = ResourceToken.AskGeometry;
                        iconBrushKey = ResourceToken.AccentBrush;
                        break;
                    case MessageBoxImage.Warning:
                        iconKey = ResourceToken.WarningGeometry;
                        iconBrushKey = ResourceToken.WarningBrush;
                        break;
                    case MessageBoxImage.Information:
                        iconKey = ResourceToken.InfoGeometry;
                        iconBrushKey = ResourceToken.InfoBrush;
                        break;
                }
            }

            if (string.IsNullOrEmpty(iconKey))
            {
                return;
            }

            messageBox.ShowImage = true;
            messageBox.Image = ResourceHelper.GetResource<Geometry>(iconKey) ?? iconGeometry;
            messageBox.ImageBrush = ResourceHelper.GetResource<IBrush>(iconBrushKey!) ?? iconBrush;
        }

        private static void ShowDialogBlocking(MessageBox messageBox, Avalonia.Controls.Window? owner)
        {
            if (owner == null)
            {
                messageBox.Show();
                return;
            }

            var signal = new ManualResetEventSlim(false);
            messageBox.Closed += (_, _) => signal.Set();
            messageBox.ShowDialog(owner);

            var dispatcher = Dispatcher.UIThread;
            while (!signal.IsSet)
            {
                dispatcher.RunJobs();
                if (!signal.IsSet)
                {
                    Thread.SpinWait(20);
                }
            }
        }
        private static bool IsValidMessageBoxButton(MessageBoxButton value) =>
            value is MessageBoxButton.OK or MessageBoxButton.OKCancel or MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel;

        private static bool IsValidMessageBoxImage(MessageBoxImage value) =>
            value is MessageBoxImage.Asterisk or MessageBoxImage.Error or MessageBoxImage.Exclamation or MessageBoxImage.Hand
                or MessageBoxImage.Information or MessageBoxImage.None or MessageBoxImage.Question or MessageBoxImage.Stop
                or MessageBoxImage.Warning;

        private static bool IsValidMessageBoxResult(MessageBoxResult value) =>
            value is MessageBoxResult.Cancel or MessageBoxResult.No or MessageBoxResult.None or MessageBoxResult.OK
                or MessageBoxResult.Yes;
    }
}