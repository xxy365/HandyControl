using System;
using System.Runtime.InteropServices;
using Avalonia.Controls.Platform;
using HandyControl.Tools.Interop;

namespace HandyControl.Controls;

public class BlurWindow : Window
{
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        EnableBlur(true);
    }

    public void EnableBlur(bool isEnabled)
    {
        try
        {
            var accentPolicy = new InteropValues.ACCENTPOLICY
            {
                AccentState = isEnabled
                    ? (int)InteropValues.ACCENTSTATE.ACCENT_ENABLE_ACRYLICBLURBEHIND
                    : (int)InteropValues.ACCENTSTATE.ACCENT_DISABLED,
                AccentFlags = 2,
                GradientColor = unchecked((int)0x99000000)
            };

            var data = new InteropValues.WINCOMPATTRDATA
            {
                Attribute = (int)InteropValues.WINDOWCOMPOSITIONATTRIB.WCA_ACCENT_POLICY,
                DataSize = Marshal.SizeOf(accentPolicy)
            };

            var size = Marshal.SizeOf(accentPolicy);
            var accentPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(accentPolicy, accentPtr, false);
                data.Data = accentPtr;

                var hwnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
                if (hwnd != IntPtr.Zero)
                {
                    InteropMethods.SetWindowCompositionAttribute(hwnd, ref data);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(accentPtr);
            }
        }
        catch
        {
        }
    }
}
