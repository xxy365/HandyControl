using System;
using Avalonia.Media;
using HandyControl.Properties.Langs;

namespace HandyControl.Data;

public class GrowlInfo
{
    public string Message { get; set; } = string.Empty;

    public bool ShowDateTime { get; set; } = true;

    public int WaitTime { get; set; } = 6;

    public string CancelStr { get; set; } = Lang.Cancel;

    public string ConfirmStr { get; set; } = Lang.Confirm;

    public Func<bool, bool>? ActionBeforeClose { get; set; }

    public bool StaysOpen { get; set; }

    public bool IsCustom { get; set; }

    public InfoType Type { get; set; }

    public Geometry? Icon { get; set; }

    public string? IconKey { get; set; }

    public IBrush? IconBrush { get; set; }

    public string? IconBrushKey { get; set; }

    public bool ShowCloseButton { get; set; } = true;

    public string? Token { get; set; }
}
