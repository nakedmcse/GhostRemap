using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

class Program
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint SCANCODE = 0x0008;

    // Scan codes
    private const ushort SC_1 = 0x02;
    private const ushort SC_2 = 0x03;
    private const ushort SC_3 = 0X04;
    private const ushort SC_R = 0x13;

    // Virtual Key Codes
    // Data from https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    private const int VK_HOME = 0x24;
    private const int VK_F = 0x46;
    private const int VK_B = 0x42;
    private const int VK_H = 0x48;
    
    // Mapping VK -> SC
    private static Dictionary<int, ushort> mapping = new Dictionary<int, ushort>()
    {
        { VK_F, SC_1 }, { VK_B, SC_2 }, { VK_H, SC_3 }
    };
    
    static IntPtr hook;
    static HookProc callback = HookCallback;
    static bool enabled = true;

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
        if (code >= 0)
        {
            int vk = Marshal.ReadInt32(lParam);
            if (vk == VK_HOME && wParam == WM_KEYDOWN)
            {
                enabled = !enabled;
                return (IntPtr)1; // swallow home
            }

            if (mapping.TryGetValue(vk, out var scode) && wParam == WM_KEYDOWN && enabled)
            {
                SendGhostCombo(scode);
                return (IntPtr)1;  // swallow key
            }
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
