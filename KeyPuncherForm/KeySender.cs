using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KeyPuncherForm
{
    /// <summary>Sends physical key presses, exactly as a real keyboard would.</summary>
    public static class KeySender
    {
        private const ushort LeftShift = 0x2A, RightAlt = 0x38, CapsLock = 0x3A;
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x1, KEYEVENTF_KEYUP = 0x2, KEYEVENTF_SCANCODE = 0x8;
        private const int EventDelayMs = 5, KeyDelayMs = 10;

        // Ctrl, Alt, Shift, Win and the letters of the hotkeys.
        private static readonly int[] HotkeyKeys = { 0x11, 0x12, 0x10, 0x5B, 0x5C, (int)Keys.V, (int)Keys.G, (int)Keys.L };

        /// <summary>Waits for the hotkey to be let go, since a held Ctrl+Alt would change every key we send.</summary>
        public static bool WaitForHotkeyRelease(TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (HotkeyKeys.Any(key => (GetAsyncKeyState(key) & 0x8000) != 0))
            {
                if (DateTime.UtcNow > deadline)
                {
                    return false;
                }

                Thread.Sleep(20);
            }

            return true;
        }

        public static void Type(IEnumerable<KeyStroke> strokes)
        {
            // Caps Lock would flip letter case and, on AZERTY, the whole number row.
            bool capsWasOn = Control.IsKeyLocked(Keys.CapsLock);
            if (capsWasOn)
            {
                Tap(CapsLock);
            }

            try
            {
                foreach (KeyStroke stroke in strokes)
                {
                    Press(stroke);
                    Thread.Sleep(KeyDelayMs);
                }
            }
            finally
            {
                if (capsWasOn)
                {
                    Tap(CapsLock);
                }
            }
        }

        private static void Press(KeyStroke stroke)
        {
            if (stroke.AltGr)
            {
                Send(RightAlt, extended: true, up: false);
            }

            if (stroke.Shift)
            {
                Send(LeftShift, extended: false, up: false);
            }

            try
            {
                Tap(stroke.ScanCode);
            }
            finally
            {
                if (stroke.Shift)
                {
                    Send(LeftShift, extended: false, up: true);
                }

                if (stroke.AltGr)
                {
                    Send(RightAlt, extended: true, up: true);
                }
            }
        }

        private static void Tap(ushort scanCode)
        {
            Send(scanCode, extended: false, up: false);
            Send(scanCode, extended: false, up: true);
        }

        private static void Send(ushort scanCode, bool extended, bool up)
        {
            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wScan = scanCode,
                        dwFlags = KEYEVENTF_SCANCODE
                            | (extended ? KEYEVENTF_EXTENDEDKEY : 0)
                            | (up ? KEYEVENTF_KEYUP : 0),
                    },
                },
            };

            if (SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>()) != 1)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            Thread.Sleep(EventDelayMs);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // MOUSEINPUT is the largest member; it is declared so the union has the size SendInput expects.
        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
    }
}
