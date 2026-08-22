using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HandyControlDemo.Tools;

public static class DemoHelper
{
    private static readonly Assembly? CodeAssembly = TryLoadCodeAssembly();

    private static Assembly? TryLoadCodeAssembly()
    {
        try
        {
            return Assembly.Load("HandyControlDemoCode");
        }
        catch
        {
            try
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "HandyControlDemoCode");
            }
            catch
            {
                return null;
            }
        }
    }

    public static string GetCode(string path)
    {
        var assembly = CodeAssembly;
        if (assembly == null || string.IsNullOrEmpty(path))
        {
            DebugLog.Log($"GetCode(\"{path}\"): assembly={assembly?.FullName ?? "NULL"}, path empty={(string.IsNullOrEmpty(path))}");
            return string.Empty;
        }

        try
        {
            var resourceName = $"HandyControlDemoCode.{path.Replace('/', '.').Replace('\\', '.')}";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                DebugLog.Log($"GetCode(\"{path}\"): resource \"{resourceName}\" NOT FOUND in {assembly.GetName().Name}");
                return string.Empty;
            }

            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            DebugLog.Log($"GetCode(\"{path}\"): OK, {content.Length} chars");
            return content;
        }
        catch (Exception ex)
        {
            DebugLog.LogException($"GetCode(\"{path}\")", ex);
            return string.Empty;
        }
    }
}
