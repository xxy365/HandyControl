using System;
using System.Runtime.InteropServices;

namespace HandyControl.Tools.Interop;

internal static class InteropMethods
{
    [DllImport(InteropValues.ExternDll.User32)]
    internal static extern IntPtr GetDesktopWindow();

    [DllImport(InteropValues.ExternDll.User32, SetLastError = true)]
    internal static extern bool GetWindowRect(IntPtr hwnd, out InteropValues.RECT rect);

    [DllImport(InteropValues.ExternDll.User32)]
    internal static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport(InteropValues.ExternDll.User32)]
    internal static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport(InteropValues.ExternDll.User32, SetLastError = true)]
    internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport(InteropValues.ExternDll.User32, CharSet = CharSet.Auto)]
    internal static extern bool GetCursorPos(out InteropValues.POINT pt);

    [DllImport(InteropValues.ExternDll.User32)]
    internal static extern bool EnableWindow(IntPtr hWnd, bool enable);

    [DllImport(InteropValues.ExternDll.User32)]
    internal static extern IntPtr ChildWindowFromPointEx(IntPtr hwndParent, InteropValues.POINT pt, int uFlags);

    [DllImport(InteropValues.ExternDll.Gdi32, SetLastError = true)]
    internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport(InteropValues.ExternDll.Gdi32, SetLastError = true)]
    internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

    [DllImport(InteropValues.ExternDll.Gdi32, ExactSpelling = true, SetLastError = true)]
    internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport(InteropValues.ExternDll.Gdi32, SetLastError = true)]
    internal static extern bool BitBlt(IntPtr hdc, int x, int y, int width, int height, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

    [DllImport(InteropValues.ExternDll.Gdi32)]
    internal static extern bool DeleteDC(IntPtr hdc);

    [DllImport(InteropValues.ExternDll.Gdi32)]
    internal static extern bool DeleteObject(IntPtr hObject);

    [DllImport(InteropValues.ExternDll.Gdi32, SetLastError = true)]
    internal static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport(InteropValues.ExternDll.Gdi32)]
    internal static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint start, uint cLines, IntPtr lpvBits, ref InteropValues.BITMAPINFO lpbmi, int usage);

    [DllImport(InteropValues.ExternDll.User32, CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(int idHook, InteropValues.HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport(InteropValues.ExternDll.User32, CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport(InteropValues.ExternDll.User32, CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport(InteropValues.ExternDll.Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport(InteropValues.ExternDll.User32)]
    internal static extern bool SetWindowCompositionAttribute(IntPtr hwnd, ref InteropValues.WINCOMPATTRDATA data);
}