using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace HandyControl.Controls;

public class TransferItem : ListBoxItem
{
    public static readonly StyledProperty<bool> IsTransferredProperty =
        AvaloniaProperty.Register<TransferItem, bool>(nameof(IsTransferred));

    public bool IsTransferred
    {
        get => GetValue(IsTransferredProperty);
        set => SetValue(IsTransferredProperty, value);
    }
}