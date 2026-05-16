using System.Runtime.InteropServices;

namespace EnvVarManager.App.Services;

public static class WindowsEnvironmentChangeNotifier
{
    private static readonly IntPtr HwndBroadcast = new(0xffff);
    private const uint WmSettingChange = 0x001A;
    private const uint SmtoAbortIfHung = 0x0002;

    public static void Notify()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        _ = SendMessageTimeout(
            HwndBroadcast,
            WmSettingChange,
            UIntPtr.Zero,
            "Environment",
            SmtoAbortIfHung,
            100,
            out _);
    }

    public static void NotifyInBackground()
    {
        _ = Task.Run(() =>
        {
            try
            {
                Notify();
            }
            catch
            {
                // 环境变量已经写入；广播通知只是尽力提醒其他程序刷新。
            }
        });
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint msg,
        UIntPtr wParam,
        string lParam,
        uint flags,
        uint timeout,
        out UIntPtr result);
}
