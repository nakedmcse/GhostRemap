using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

class Program
{
    const int WH_KEYBOARD_LL = 13;
    const int WM_KEYDOWN = 0x0100;
    const int WM_KEYUP = 0x0101;
    const uint KEYEVENTF_KEYUP = 0x0002;
    const uint SCANCODE = 0x0008;

    // Scan codes
    const ushort SC_1 = 0x02;
    const ushort SC_2 = 0x03;
    const ushort SC_3 = 0X04;
    const ushort SC_R = 0x13;
    const ushort SC_F = 0x21;
    const ushort SC_G = 0x22;
    const ushort SC_V = 0x2F;

    static IntPtr hook;
    static HookProc callback = HookCallback;

    [STAThread]
    static void Main()
    {
        hook = SetWindowsHookEx(
            WH_KEYBOARD_LL,
            callback,
            GetModuleHandle(Process.GetCurrentProcess().MainModule!.ModuleName),
            0);

        Application.Run();
    }

    static IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0 && Marshal.ReadInt32(lParam) == 0x46) // F
        {
            if (wParam == WM_KEYDOWN) SendGhostCombo(SC_1);
            return (IntPtr)1; // swallow F
        }

        if (code >= 0 && Marshal.ReadInt32(lParam) == 0x56) // V
        {
            if (wParam == WM_KEYDOWN) SendGhostCombo(SC_2);
            return (IntPtr)1; // swallow V
        }

        if (code >= 0 && Marshal.ReadInt32(lParam) == 0x42) // B
        {
            if (wParam == WM_KEYDOWN) SendGhostCombo(SC_3);
            return (IntPtr)1; // swallow B
        }

        return CallNextHookEx(hook, code, wParam, lParam);
    }

    static void SendGhostCombo(ushort scode)
    {
       INPUT[] downR =
        {
            Key(SC_R, SCANCODE)
        };
        SendInput((uint)downR.Length, downR, Marshal.SizeOf<INPUT>());
        Thread.Sleep(10);

        INPUT[] down1 =
        {
            Key(scode, SCANCODE)
        };
        SendInput((uint)down1.Length, down1, Marshal.SizeOf<INPUT>());
        Thread.Sleep(10);

        INPUT[] up =
        {
            Key(scode, SCANCODE | KEYEVENTF_KEYUP),
            Key(SC_R, SCANCODE | KEYEVENTF_KEYUP)
        };
        SendInput((uint)up.Length, up, Marshal.SizeOf<INPUT>());
    }

     static INPUT Key(ushort key, uint flags) => new()
    {
        type = 1,
        U = new InputUnion
        {
            ki = new KEYBDINPUT { wScan = key, dwFlags = flags }
        }
    };

    delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

     [StructLayout(LayoutKind.Sequential)]
    struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [DllImport("user32.dll")]
    static extern uint SendInput(uint count, INPUT[] inputs, int size);

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowsHookEx(
        int idHook, HookProc callback, IntPtr module, uint threadId);

    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(
        IntPtr hook, int code, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandle(string? moduleName);
}
