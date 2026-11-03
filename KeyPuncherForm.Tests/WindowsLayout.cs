using System.Runtime.InteropServices;
using System.Text;

namespace KeyPuncherForm.Tests
{
    /// <summary>Asks the real Windows keyboard driver what a key sequence produces, without pressing anything.</summary>
    internal sealed class WindowsLayout : IDisposable
    {
        private const uint KLF_NOTELLSHELL = 0x80;
        private const uint MAPVK_VSC_TO_VK = 1;
        private const int VK_SHIFT = 0x10, VK_CONTROL = 0x11, VK_MENU = 0x12, VK_SPACE = 0x20;

        private readonly IntPtr _hkl;
        private readonly bool _wasLoaded;

        public WindowsLayout(string layoutId)
        {
            var list = new IntPtr[64];
            int count = GetKeyboardLayoutList(list.Length, list);

            _hkl = LoadKeyboardLayout(layoutId, KLF_NOTELLSHELL);
            if (((long)_hkl & 0xFFFF) != Convert.ToInt32(layoutId, 16))
            {
                throw new InvalidOperationException($"Windows did not load keyboard layout {layoutId}.");
            }

            _wasLoaded = list.Take(count).Contains(_hkl);
        }

        /// <summary>The text a sequence of key presses yields.</summary>
        public string Produce(IEnumerable<KeyStroke> strokes)
        {
            Reset();
            var output = new StringBuilder();
            foreach (KeyStroke stroke in strokes)
            {
                output.Append(Translate(stroke, out _));
            }

            Reset();
            return output.ToString();
        }

        public bool IsDead(KeyStroke stroke)
        {
            Reset();
            Translate(stroke, out bool isDead);
            Reset();
            return isDead;
        }

        public void Dispose()
        {
            if (!_wasLoaded)
            {
                UnloadKeyboardLayout(_hkl);
            }
        }

        private string Translate(KeyStroke stroke, out bool isDead)
        {
            var state = new byte[256];
            if (stroke.Shift)
            {
                state[VK_SHIFT] = 0x80;
            }

            if (stroke.AltGr)
            {
                state[VK_CONTROL] = 0x80;
                state[VK_MENU] = 0x80;
            }

            uint vk = MapVirtualKeyEx(stroke.ScanCode, MAPVK_VSC_TO_VK, _hkl);
            var buffer = new StringBuilder(8);
            int count = ToUnicodeEx(vk, stroke.ScanCode, state, buffer, buffer.Capacity, 0, _hkl);
            isDead = count < 0;
            return count > 0 ? buffer.ToString(0, count) : "";
        }

        private void Reset()
        {
            // Pending dead-key state is flushed by pressing Space until the driver stops buffering.
            var buffer = new StringBuilder(8);
            while (ToUnicodeEx(VK_SPACE, 0x39, new byte[256], buffer, buffer.Capacity, 0, _hkl) < 0)
            {
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadKeyboardLayout(string id, uint flags);

        [DllImport("user32.dll")]
        private static extern bool UnloadKeyboardLayout(IntPtr hkl);

        [DllImport("user32.dll")]
        private static extern int GetKeyboardLayoutList(int count, [Out] IntPtr[] list);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyEx(uint code, uint mapType, IntPtr hkl);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int ToUnicodeEx(uint vk, uint scan, byte[] state, StringBuilder buffer, int size, uint flags, IntPtr hkl);
    }
}
