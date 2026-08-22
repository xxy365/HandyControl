using System;
using System.IO;

namespace HandyControlDemo.Tools;

public static class DebugLog
{
    private static readonly object Lock = new();
    private static readonly string LogFile =
        Path.Combine(Path.GetTempPath(), "HandyControlDemo_code_debug.log");

    public static string LogFileFullPath => LogFile;

    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        lock (Lock)
        {
            try
            {
                File.AppendAllText(LogFile, line + Environment.NewLine);
            }
            catch
            {
                // ignore
            }
        }
        System.Diagnostics.Debug.WriteLine("[CodeDebug] " + line);
        Console.WriteLine("[CodeDebug] " + line);
    }

    public static void LogException(string context, Exception ex)
    {
        Log($"{context} EXCEPTION: {ex.GetType().FullName}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
    }

    public static void SessionStart()
    {
        Log($"===== SESSION START pid={Environment.ProcessId} base={AppContext.BaseDirectory} =====");
        Log($"log file: {LogFile}");
    }
}
