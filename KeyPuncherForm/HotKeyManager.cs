using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

 
namespace KeyPuncherForm
{
   
    public class HotKeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 1;
        private IntPtr _windowHandle;
        private uint _modifier;
        private uint _key;

        public HotKeyManager(IntPtr windowHandle, uint modifier, Keys key)
        {
            _windowHandle = windowHandle;
            _modifier = modifier;
            _key = (uint)key;

            RegisterHotKey(_windowHandle, HOTKEY_ID, _modifier, _key);
        }
            
        public void Unregister()
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
        }
    }

}
