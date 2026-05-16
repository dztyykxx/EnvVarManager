using System.Runtime.InteropServices;

namespace EnvVarManager.Core;

internal static class WindowsEnvironmentChangeNotifier
{
    private const int BroadcastWindow = 0xffff;
    private const int SettingChangeMessage = 0x001A;
    private const int AbortIfHung = 0x0002;
    private const int TimeoutMilliseconds = 100;

    public static void Queue()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        _ = Task.Run(Notify);
    }

    private static void Notify()
    {
        try
        {
            _ = SendMessageTimeout(
                new IntPtr(BroadcastWindow),
                SettingChangeMessage,
                IntPtr.Zero,
                "Environment",
                AbortIfHung,
                TimeoutMilliseconds,
                out _);
        }
        catch (DllNotFoundException)
        {
            // Registry writes are authoritative; this notification is best effort.
        }
        catch (EntryPointNotFoundException)
        {
            // Registry writes are authoritative; this notification is best effort.
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        string lParam,
        int flags,
        int timeout,
        out IntPtr result);
}
