using System;

namespace HandyControl.Tools;

/// <summary>
///     Contains some regular expression validation operation required strings.
/// </summary>
public sealed class RegexPatterns
{
    public const string MailPattern =
        @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

    public const string PhonePattern = @"^((13[0-9])|(15[^4,\d])|(18[0,5-9]))\d{8}$";

    public const string IpPattern =
        @"^(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";

    public const string IpAPattern =
        @"^(12[0-6]|1[0-1]\d|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";

    public const string IpBPattern =
        @"^(19[0-1]|12[8-9]|1[3-8]\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";

    public const string IpCPattern =
        @"^(19[2-9]|22[0-3]|2[0-1]\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";

    public const string IpDPattern =
        @"^(22[4-9]|23\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";

    public const string IpEPattern =
        @"^(25[0-5]|24\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\."
        + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";

    public const string ChinesePattern = @"^[\u4e00-\u9fa5]$";

    public const string UrlPattern =
        @"((http|ftp|https)://)(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,4})*(/[a-zA-Z0-9\&%_\./-~-]*)?";

    public const string NumberPattern = @"^\d+$";

    public const string DigitsPattern = @"[+-]?\d*\.?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?";

    public const string PIntPattern = @"^[1-9]\d*$";

    public const string NIntPattern = @"^-[1-9]\d*$";

    public const string IntPattern = @"^-?[1-9]\d*|0$";

    public const string NnIntPattern = @"^[1-9]\d*|0$";

    public const string NpIntPattern = @"^-[1-9]\d*|0$";

    public const string PDoublePattern = @"^[1-9]\d*\.\d*|0\.\d*[1-9]\d*$";

    public const string NDoublePattern = @"^-([1-9]\d*\.\d*|0\.\d*[1-9]\d*)$";

    public const string DoublePattern = @"^-?([1-9]\d*\.\d*|0\.\d*[1-9]\d*|0?\.0+|0)$";

    public const string NnDoublePattern = @"^[1-9]\d*\.\d*|0\.\d*[1-9]\d*|0?\.0+|0$";

    public const string NpDoublePattern = @"^(-([1-9]\d*\.\d*|0\.\d*[1-9]\d*))|0?\.0+|0$";

    public object? GetValue(string propertyName) => propertyName switch
    {
        nameof(MailPattern) => MailPattern,
        nameof(PhonePattern) => PhonePattern,
        nameof(IpPattern) => IpPattern,
        nameof(IpAPattern) => IpAPattern,
        nameof(IpBPattern) => IpBPattern,
        nameof(IpCPattern) => IpCPattern,
        nameof(IpDPattern) => IpDPattern,
        nameof(IpEPattern) => IpEPattern,
        nameof(ChinesePattern) => ChinesePattern,
        nameof(UrlPattern) => UrlPattern,
        nameof(NumberPattern) => NumberPattern,
        nameof(DigitsPattern) => DigitsPattern,
        nameof(PIntPattern) => PIntPattern,
        nameof(NIntPattern) => NIntPattern,
        nameof(IntPattern) => IntPattern,
        nameof(NnIntPattern) => NnIntPattern,
        nameof(NpIntPattern) => NpIntPattern,
        nameof(PDoublePattern) => PDoublePattern,
        nameof(NDoublePattern) => NDoublePattern,
        nameof(DoublePattern) => DoublePattern,
        nameof(NnDoublePattern) => NnDoublePattern,
        nameof(NpDoublePattern) => NpDoublePattern,
        _ => null
    };
}
