using System.Runtime.InteropServices;
using System.Text;

namespace EnvVarManager.App.Services;

public static class SafeClipboard
{
    private const uint CfUnicodeText = 13;
    private const uint GmemMoveable = 0x0002;

    public static bool TrySetText(string text, out string errorMessage)
    {
        errorMessage = "";

        if (!OperatingSystem.IsWindows())
        {
            errorMessage = "当前平台不支持写入 Windows 剪贴板。";
            return false;
        }

        var bytes = Encoding.Unicode.GetBytes(text + '\0');
        var handle = GlobalAlloc(GmemMoveable, (UIntPtr)bytes.Length);
        if (handle == IntPtr.Zero)
        {
            errorMessage = "复制失败：无法分配剪贴板内存。";
            return false;
        }

        var clipboardOpened = false;
        var clipboardOwnsHandle = false;

        try
        {
            var target = GlobalLock(handle);
            if (target == IntPtr.Zero)
            {
                errorMessage = "复制失败：无法写入剪贴板内存。";
                return false;
            }

            Marshal.Copy(bytes, 0, target, bytes.Length);
            _ = GlobalUnlock(handle);

            if (!OpenClipboard(IntPtr.Zero))
            {
                errorMessage = "剪贴板暂时不可用，可能正被其他程序占用，请稍后再试。";
                return false;
            }

            clipboardOpened = true;

            if (!EmptyClipboard())
            {
                errorMessage = "复制失败：无法清空剪贴板。";
                return false;
            }

            if (SetClipboardData(CfUnicodeText, handle) == IntPtr.Zero)
            {
                errorMessage = "复制失败：无法写入剪贴板。";
                return false;
            }

            clipboardOwnsHandle = true;
            return true;
        }
        finally
        {
            if (clipboardOpened)
            {
                _ = CloseClipboard();
            }

            if (!clipboardOwnsHandle)
            {
                _ = GlobalFree(handle);
            }
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr hMem);
}
