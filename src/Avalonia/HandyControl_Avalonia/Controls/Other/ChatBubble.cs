using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using HandyControl.Data;

namespace HandyControl.Controls;

public class ChatBubble : SelectableItem
{
    private const string ElementTail = "Tail";

    private const string ElementBody = "Body";

    private const string ElementAudio = "Audio";

    private const string ElementDot = "Dot";

    private Path? _tail;

    private Border? _body;

    private Path? _audio;

    private Ellipse? _dot;

    public static readonly StyledProperty<ChatRoleType> RoleProperty =
        AvaloniaProperty.Register<ChatBubble, ChatRoleType>(nameof(Role));

    public ChatRoleType Role
    {
        get => GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    public static readonly StyledProperty<ChatMessageType> TypeProperty =
        AvaloniaProperty.Register<ChatBubble, ChatMessageType>(nameof(Type));

    public ChatMessageType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly StyledProperty<bool> IsReadProperty =
        AvaloniaProperty.Register<ChatBubble, bool>(nameof(IsRead));

    public bool IsRead
    {
        get => GetValue(IsReadProperty);
        set => SetValue(IsReadProperty, value);
    }

    public Action<object>? ReadAction { get; set; }

    protected override void OnSelected(RoutedEventArgs e)
    {
        base.OnSelected(e);

        IsRead = true;
        ReadAction?.Invoke(Content!);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _tail = e.NameScope.Find<Path>(ElementTail);
        _body = e.NameScope.Find<Border>(ElementBody);
        _audio = e.NameScope.Find<Path>(ElementAudio);
        _dot = e.NameScope.Find<Ellipse>(ElementDot);

        UpdateBubbleLayout();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RoleProperty || change.Property == TypeProperty || change.Property == IsReadProperty)
        {
            UpdateBubbleLayout();
        }
    }

    private void UpdateBubbleLayout()
    {
        if (_tail != null)
        {
            if (Role == ChatRoleType.Receiver)
            {
                _tail.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                _tail.RenderTransform = new ScaleTransform(-1, 1);
                if (_body != null)
                {
                    _body.Margin = new Thickness(3, 0, 0, 0);
                }
            }
            else
            {
                _tail.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                _tail.RenderTransform = null;
                if (_body != null)
                {
                    _body.Margin = new Thickness(0, 0, 3, 0);
                }
            }
        }

        if (_audio != null)
        {
            _audio.IsVisible = Type == ChatMessageType.Audio;
        }

        if (_dot != null)
        {
            _dot.IsVisible = Type == ChatMessageType.Audio && !IsRead;
        }
    }
}