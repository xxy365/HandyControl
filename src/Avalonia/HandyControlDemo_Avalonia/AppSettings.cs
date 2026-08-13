using System;
using System.IO;
using Newtonsoft.Json;

namespace HandyControlDemo;

/// <summary>
///     应用设置的持久化（主题、语言）。
/// </summary>
public static class AppSettings
{
    private const string FileName = "settings.json";

    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HandyControlDemo");

    private static readonly string FilePath = Path.Combine(DirectoryPath, FileName);

    /// <summary>
    ///     主题变体名称：Default / Dark / Light。
    /// </summary>
    public static string ThemeVariant { get; set; } = "Default";

    /// <summary>
    ///     语言标签，如 en / zh-cn。
    /// </summary>
    public static string Language { get; set; } = "en";

    public static void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return;
            }

            var json = File.ReadAllText(FilePath);
            var settings = JsonConvert.DeserializeObject<SettingsData>(json);
            if (settings == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(settings.ThemeVariant))
            {
                ThemeVariant = settings.ThemeVariant;
            }

            if (!string.IsNullOrEmpty(settings.Language))
            {
                Language = settings.Language;
            }
        }
        catch
        {
            // 设置损坏时忽略，使用默认值
        }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            var json = JsonConvert.SerializeObject(new SettingsData
            {
                ThemeVariant = ThemeVariant,
                Language = Language
            }, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // 写入失败时忽略
        }
    }

    private class SettingsData
    {
        public string ThemeVariant { get; set; } = "Default";

        public string Language { get; set; } = "en";
    }
}
