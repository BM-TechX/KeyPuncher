using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace KeyPuncherForm.Tests
{
    /// <summary>
    /// Presses real keys into a window of its own, so it is opt-in and needs an unattended desktop for a minute.
    /// Set KEYPUNCHER_E2E_APP to the path of a built KeyPuncher exe to run it. Typing stops if the window loses focus.
    /// </summary>
    public sealed class EndToEndFactAttribute : FactAttribute
    {
        public EndToEndFactAttribute()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KEYPUNCHER_E2E_APP")))
            {
                Skip = "Set KEYPUNCHER_E2E_APP to run tests that press real keys.";
            }
        }
    }

    public class EndToEndTests
    {
        private const uint KEYUP = 2;
        private const byte VK_CONTROL = 0x11, VK_MENU = 0x12, VK_CAPITAL = 0x14, VK_V = 0x56;

        private readonly StringBuilder _failures = new();
        private Form _form = null!;
        private TextBox _box = null!;
        private IntPtr _handle;

        [EndToEndFact]
        public void TypedTextArrivesExactly()
        {
            var ui = new Thread(RunWindow);
            ui.SetApartmentState(ApartmentState.STA);
            ui.Start();
            ui.Join();
            Assert.True(_failures.Length == 0, _failures.ToString());
        }

        private void RunWindow()
        {
            _form = new Form { Text = "KeyPuncher test - do not touch", TopMost = true, Width = 800, Height = 400 };
            _box = new TextBox { Multiline = true, AcceptsTab = true, AcceptsReturn = true, Dock = DockStyle.Fill };
            _form.Controls.Add(_box);
            _form.Shown += (_, _) =>
            {
                _handle = _form.Handle;
                // Tapping Alt lets a background process take the foreground.
                keybd_event(VK_MENU, 0x38, 0, IntPtr.Zero);
                keybd_event(VK_MENU, 0x38, KEYUP, IntPtr.Zero);
                SetForegroundWindow(_handle);
                _box.Focus();
                var worker = new Thread(RunChecks) { IsBackground = true };
                worker.SetApartmentState(ApartmentState.STA);
                worker.Start();
            };
            Application.Run(_form);
        }

        private void RunChecks()
        {
            Thread.Sleep(500);
            IntPtr userLayout = _form.Invoke(() => InputLanguage.CurrentInputLanguage.Handle);
            var loaded = new List<IntPtr>();
            try
            {
                var samples = new Dictionary<KeyboardLayout, string>
                {
                    [KeyboardLayout.Danish] = "Rød grød med fløde, ÆØÅ æøå: \"a@b.dk\" \\ | ~ ^ ` ´ ¨ $ € £\nC:\\temp\\f.txt {a[1]} ",
                    [KeyboardLayout.EnglishUs] = "user@host:~$ echo \"a\\b\" | grep -E '^[x]{2}$' && `ls` # 100%\n",
                    [KeyboardLayout.French] = "Où ça ? Déjà l'été, ÊTRE à côté de Noël : 100 % ~ `x` ^ @ #\nC:\\temp\\f.txt {a[1]} | €5 ",
                };

                foreach (var (layout, sample) in samples)
                {
                    loaded.Add(Activate(layout));
                    // µ is left out because Danish AltGr+M is Ctrl+Alt+M, a common global hotkey that swallows the key.
                    string all = new(layout.Characters.Keys.Where(c => c is not ('\t' or 'µ')).Order().ToArray());
                    TypeAndCheck(layout.Name, layout, sample + all, sample + all);
                }

                CheckCapsLock();
                _form.Invoke(() => ActivateKeyboardLayout(userLayout, 0));
                CheckHotkeyThroughRealApp();
            }
            catch (Exception e)
            {
                _failures.AppendLine("Aborted: " + e.Message);
            }
            finally
            {
                _form.Invoke(() => ActivateKeyboardLayout(userLayout, 0));
                foreach (IntPtr layout in loaded.Where(h => h != userLayout))
                {
                    UnloadKeyboardLayout(layout);
                }

                _form.Invoke(_form.Close);
            }
        }

        private IntPtr Activate(KeyboardLayout layout)
        {
            IntPtr hkl = LoadKeyboardLayout(layout.WindowsLayoutId, 0x80);
            _form.Invoke(() => ActivateKeyboardLayout(hkl, 0));
            Thread.Sleep(100);
            return hkl;
        }

        private void TypeAndCheck(string label, KeyboardLayout layout, string text, string expected)
        {
            ClearBox();
            KeySender.Type(WhileFocused(layout.Plan(text).Strokes));
            Thread.Sleep(300);
            Compare(label, expected);
        }

        private void CheckCapsLock()
        {
            TapCapsLock();
            try
            {
                TypeAndCheck("Caps Lock on", KeyboardLayout.French, "Abc 123 é", "Abc 123 é");
                if (!Control.IsKeyLocked(Keys.CapsLock))
                {
                    _failures.AppendLine("Caps Lock was not turned back on.");
                }
            }
            finally
            {
                TapCapsLock();
            }
        }

        private void CheckHotkeyThroughRealApp()
        {
            using Process app = Process.Start(Environment.GetEnvironmentVariable("KEYPUNCHER_E2E_APP")!);
            Thread.Sleep(2500);
            string? saved = _form.Invoke(() => Clipboard.ContainsText() ? Clipboard.GetText() : null);
            try
            {
                _form.Invoke(() =>
                {
                    Clipboard.SetText("Hello\r\nRød “grød”\tmed a b c ✓ ç @\\");
                    _form.Activate();
                    SetForegroundWindow(_handle);
                });
                Thread.Sleep(300);
                ClearBox();
                ThrowIfUnfocused();

                keybd_event(VK_CONTROL, 0x1D, 0, IntPtr.Zero);
                keybd_event(VK_MENU, 0x38, 0, IntPtr.Zero);
                keybd_event(VK_V, 0x2F, 0, IntPtr.Zero);
                Thread.Sleep(600); // Held on purpose: typing must wait for the release.
                keybd_event(VK_V, 0x2F, KEYUP, IntPtr.Zero);
                keybd_event(VK_MENU, 0x38, KEYUP, IntPtr.Zero);
                keybd_event(VK_CONTROL, 0x1D, KEYUP, IntPtr.Zero);
                Thread.Sleep(3000);

                // App picks its layout from this machine's language; the test is only meaningful on a Danish one.
                Compare("Hotkey through the app", "Hello\nRød \"grød\"\tmed a b c  c @\\");
            }
            finally
            {
                app.Kill();
                _form.Invoke(() =>
                {
                    if (saved != null)
                    {
                        Clipboard.SetText(saved);
                    }
                    else
                    {
                        Clipboard.Clear();
                    }
                });
            }
        }

        private void Compare(string label, string expected)
        {
            string actual = _form.Invoke(() => _box.Text).Replace("\r\n", "\n");
            expected = expected.Replace("\r\n", "\n");
            if (actual != expected)
            {
                _failures.AppendLine($"{label}\n expected: {expected}\n actual:   {actual}");
            }
        }

        private void ClearBox()
        {
            _form.Invoke(() =>
            {
                _box.Clear();
                _box.Focus();
            });
            Thread.Sleep(100);
        }

        private IEnumerable<KeyStroke> WhileFocused(IEnumerable<KeyStroke> strokes)
        {
            foreach (KeyStroke stroke in strokes)
            {
                ThrowIfUnfocused();
                yield return stroke;
            }
        }

        private void ThrowIfUnfocused()
        {
            if (GetForegroundWindow() != _handle)
            {
                throw new InvalidOperationException("The test window lost focus, so typing stopped.");
            }
        }

        private static void TapCapsLock()
        {
            keybd_event(VK_CAPITAL, 0x3A, 0, IntPtr.Zero);
            keybd_event(VK_CAPITAL, 0x3A, KEYUP, IntPtr.Zero);
            Thread.Sleep(100);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr window);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadKeyboardLayout(string id, uint flags);

        [DllImport("user32.dll")]
        private static extern bool UnloadKeyboardLayout(IntPtr hkl);

        [DllImport("user32.dll")]
        private static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, uint flags);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte vk, byte scan, uint flags, IntPtr extra);
    }
}
