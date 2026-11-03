namespace KeyPuncherForm
{
    public partial class Form1 : Form
    {
        private HotKeyManager _hotKeyManager;
        private bool _isWritingEnabled = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Dock = DockStyle.Fill;
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

                richTextBox1.Text = HideText.Checked ? "************" : text;
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.Refresh();

                if (!KeySender.WaitForHotkeyRelease(TimeSpan.FromSeconds(10)))
                {
                    richTextBox1.AppendText("Nothing typed: the hotkey was held down for too long." + Environment.NewLine);
                    return;
                }

                System.Threading.Thread.Sleep(200);
                KeySender.Type(plan.Strokes);
                ReportProblem("typed without accent", plan.WithoutAccent);
                ReportProblem("not typed", plan.Skipped);
            }
        }

        private void ReportProblem(string problem, IReadOnlyList<string> characters)
        {
            if (characters.Count > 0)
            {
                // The characters stay out of the window while the text is hidden, as they may be part of a password.
                string detail = HideText.Checked ? "" : ": " + string.Join(" ", characters.Distinct());
                richTextBox1.AppendText($"Warning: {characters.Count} character(s) {problem}{detail}{Environment.NewLine}");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _hotKeyManager.Unregister();
            base.OnFormClosed(e);
        }
    }
}
