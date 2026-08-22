using Avalonia;

namespace HandyControl.Controls;

public class PasswordBoxAttach
{
    /// <summary>
    ///     密码长度
    /// </summary>
    public static readonly AttachedProperty<int> PasswordLengthProperty =
        AvaloniaProperty.RegisterAttached<PasswordBoxAttach, AvaloniaObject, int>("PasswordLength");

    public static void SetPasswordLength(AvaloniaObject element, int value) =>
        element.SetValue(PasswordLengthProperty, value);

    public static int GetPasswordLength(AvaloniaObject element) =>
        element.GetValue(PasswordLengthProperty);

    /// <summary>
    ///     是否监测
    /// </summary>
    public static readonly AttachedProperty<bool> IsMonitoringProperty =
        AvaloniaProperty.RegisterAttached<PasswordBoxAttach, AvaloniaObject, bool>("IsMonitoring",
            inherits: true);

    public static void SetIsMonitoring(AvaloniaObject element, bool value) =>
        element.SetValue(IsMonitoringProperty, value);

    public static bool GetIsMonitoring(AvaloniaObject element) =>
        element.GetValue(IsMonitoringProperty);

    static PasswordBoxAttach()
    {
        IsMonitoringProperty.Changed.AddClassHandler<AvaloniaObject>(OnIsMonitoringChanged);
    }

    private static void OnIsMonitoringChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
        {
            return;
        }

        if (e.NewValue is bool boolValue)
        {
            if (boolValue)
            {
                passwordBox.PasswordChanged += PasswordChanged;
            }
            else
            {
                passwordBox.PasswordChanged -= PasswordChanged;
            }
        }
    }

    private static void PasswordChanged(object? sender, System.EventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            SetPasswordLength(passwordBox, passwordBox.Password.Length);
        }
    }
}