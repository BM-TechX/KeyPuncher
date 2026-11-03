using System.Runtime.InteropServices;

namespace KeyPuncherForm
{
    public partial class Form1 : Form
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private HotKeyManager _hotKeyManager;
        private bool _isWritingEnabled = true;

        public Form1()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int enabled = 1;
            DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref enabled, sizeof(int));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = "Your copied text shows up here.";
            LayoutBox.Items.AddRange(KeyboardLayout.All.ToArray<object>());
            LayoutBox.SelectedItem = KeyboardLayout.ForLanguage(InputLanguage.CurrentInputLanguage.Culture.TwoLetterISOLanguageName);
            _hotKeyManager = new HotKeyManager(this.Handle, 3, Keys.V);
            _hotKeyManager = new HotKeyManager(this.Handle, 3, Keys.G); // Additional hotkey for stopping/resuming
            _hotKeyManager = new HotKeyManager(this.Handle, 3, Keys.L);

        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312) // WM_HOTKEY
            {
                if ((int)m.WParam == 1) // ID for CTRL+ALT+V
                {
                    HandlePasteHotKey();
                }
                else if ((int)m.WParam == 2) // ID for CTRL+ALT+S (assuming you set this ID for stop/resume hotkey)
                {
                    _isWritingEnabled = !_isWritingEnabled; // Toggle writing state
                }
            }
        }

        private void HandlePasteHotKey()
        {
            if (_isWritingEnabled && Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                TypingPlan plan = ((KeyboardLayout)LayoutBox.SelectedItem!).Plan(text);

                richTextBox1.ForeColor = Theme.Text;
                richTextBox1.Text = HideText.Checked ? "************" : text;
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.Refresh();
                Strip.Progress = 0;
                SetStatus("Let go of the keys", Theme.Amber);

                if (!KeySender.WaitForHotkeyRelease(TimeSpan.FromSeconds(10)))
                {
                    AppendWarning("Nothing typed: the hotkey was held down for too long.");
                    SetStatus("Nothing typed", Theme.Coral);
                    return;
                }

                SetStatus("Typing…", Theme.Amber);
                System.Threading.Thread.Sleep(200);
                KeySender.Type(WithProgress(plan.Strokes));
                Strip.Progress = 1;

                ReportProblem("typed without accent", plan.WithoutAccent);
                ReportProblem("not typed", plan.Skipped);
                bool hasProblems = plan.WithoutAccent.Count > 0 || plan.Skipped.Count > 0;
                SetStatus(hasProblems ? "Done, with warnings" : "Done", hasProblems ? Theme.Coral : Theme.Mint);
            }
        }

        private IEnumerable<KeyStroke> WithProgress(IReadOnlyList<KeyStroke> strokes)
        {
            for (int i = 0; i < strokes.Count; i++)
            {
                yield return strokes[i];
                Strip.Progress = (i + 1f) / strokes.Count;
            }
        }

        private void SetStatus(string text, Color color)
        {
            StatusLabel.Text = "●  " + text;
            StatusLabel.ForeColor = color;
            StatusLabel.Refresh();
        }

        private void ReportProblem(string problem, IReadOnlyList<string> characters)
        {
            if (characters.Count > 0)
            {
                // The characters stay out of the window while the text is hidden, as they may be part of a password.
                string detail = HideText.Checked ? "" : ": " + string.Join(" ", characters.Distinct());
                AppendWarning($"Warning: {characters.Count} character(s) {problem}{detail}");
            }
        }

        private void AppendWarning(string message)
        {
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionColor = Theme.Coral;
            richTextBox1.AppendText(message + Environment.NewLine);
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _hotKeyManager.Unregister();
            base.OnFormClosed(e);
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);
    }
}
