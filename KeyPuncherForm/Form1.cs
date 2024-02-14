namespace KeyPuncherForm
{
    using GregsStack.InputSimulatorStandard;
    using GregsStack.InputSimulatorStandard.Native;

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
                int check = 1;
                if (EngLang.Checked)
                {
                    check = 0;
                }
                var inputSimulator = new InputSimulator();
                string text = Clipboard.GetText();
                // Update RichTextBox with copied text
                richTextBox1.Text = "";
                if (HideText.Checked)
                {
                    richTextBox1.AppendText("************" + Environment.NewLine);
                }
                else {
                    richTextBox1.AppendText(text + Environment.NewLine);
                }
                
                System.Threading.Thread.Sleep(1000);
                // Simulate typing the copied text
                foreach (char c in text)
                {
                    // Direct simulation for alphabetical and numerical characters
                    SimulateKeyPress(c, inputSimulator, check);
                    System.Threading.Thread.Sleep(10); // Mimic natural typing speed - 90ms
                }
            }
        }
        private void SimulateKeyPress(char c, InputSimulator inputSimulator, int lang = 0)
        {
            bool shiftNeeded = false;
            GregsStack.InputSimulatorStandard.Native.VirtualKeyCode keyCodeKey = new GregsStack.InputSimulatorStandard.Native.VirtualKeyCode();
            if (lang == 0)
            {
                switch (c)
                {
                    case '!':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_1;
                        break;
                    case '@':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_2;
                        break;
                    case '#':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_3;
                        break;
                    case '$':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_4;
                        break;
                    case '%':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_5;
                        break;
                    case '^':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_6;
                        break;
                    case '&':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_7;
                        break;
                    case '*':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_8;
                        break;
                    case '(':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_9;
                        break;
                    case ')':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_0;
                        break;
                    case '-':
                        keyCodeKey = VirtualKeyCode.OEM_MINUS;
                        break;
                    case '_':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_MINUS;
                        break;
                    case '=':
                        keyCodeKey = VirtualKeyCode.OEM_PLUS;
                        break;
                    case '+':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_PLUS;
                        break;
                    case '[':
                        keyCodeKey = VirtualKeyCode.OEM_4;
                        break;
                    case '{':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_4;
                        break;
                    case ']':
                        keyCodeKey = VirtualKeyCode.OEM_6;
                        break;
                    case '}':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_6;
                        break;
                    case '\\':
                        keyCodeKey = VirtualKeyCode.OEM_5;
                        break;
                    case '|':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_5;
                        break;
                    case ';':
                        keyCodeKey = VirtualKeyCode.OEM_1;
                        break;
                    case ':':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_1;
                        break;
                    case '\'':
                        keyCodeKey = VirtualKeyCode.OEM_7;
                        break;
                    case '\"':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_7;
                        break;
                    case ',':
                        keyCodeKey = VirtualKeyCode.OEM_COMMA;
                        break;
                    case '<':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_COMMA;
                        break;
                    case '.':
                        keyCodeKey = VirtualKeyCode.OEM_PERIOD;
                        break;
                    case '>':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_PERIOD;
                        break;
                    case '/':
                        keyCodeKey = VirtualKeyCode.OEM_2;
                        break;
                    case '?':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_2;
                        break;
                    case '`':
                        keyCodeKey = VirtualKeyCode.OEM_3;
                        break;
                    case '~':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_3;
                        break;
                    // Add more cases as needed for other characters
                    default:
                        // Fallback for letters, digits, and unhandled characters
                        // Implementation for ConvertToVirtualKeyCode not shown
                        shiftNeeded = char.IsUpper(c);
                        var converted = ConvertToVirtualKeyCode(c);
                        if (converted.HasValue)
                        {
                            keyCodeKey = converted.Value;
                        }
                        else
                        {
                            // Directly append unhandled characters
                            // Ensure richTextBox1 or similar output is available and correctly handled
                            if (HideText.Checked)
                            {
                                richTextBox1.AppendText("*");
                            }
                            else
                            {
                                richTextBox1.AppendText(c.ToString());
                            }
                            
                        }
                        break;
                }
            }
            else if (lang == 1)
            {
                //danish keyboard
                switch (c)
                {
                    case '!':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_1;
                        break;
                    case '"':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_2;
                        break;
                    case '#':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_3;
                        break;
                    case '¤':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_4;
                        break;
                    case '%':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_5;
                        break;
                    case '&':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_6;
                        break;
                    case '/':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_7;
                        break;
                    case '(':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_8;
                        break;
                    case ')':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_9;
                        break;
                    case '=':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_0;
                        break;
                    case '?':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_PLUS;
                        break;
                    case '`':
                        keyCodeKey = VirtualKeyCode.OEM_MINUS;
                        break;
                    case '+':
                        keyCodeKey = VirtualKeyCode.OEM_PLUS;
                        break;
                    case '*':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_PLUS;
                        break;
                    case 'Å':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_6;
                        break;
                    case 'å':
                        keyCodeKey = VirtualKeyCode.VK_6;
                        break;
                    case 'Æ':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_7;
                        break;
                    case 'æ':
                        keyCodeKey = VirtualKeyCode.VK_7;
                        break;
                    case 'Ø':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_0;
                        break;
                    case 'ø':
                        keyCodeKey = VirtualKeyCode.VK_0;
                        break;
                    case '<':
                        keyCodeKey = VirtualKeyCode.OEM_102;
                        break;
                    case '>':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_102;
                        break;
                    case '\\':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_5;
                        break;
                    case '|':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_102;
                        break;
                    case '@':
                        keyCodeKey = VirtualKeyCode.VK_2;
                        break;
                    case '£':
                        keyCodeKey = VirtualKeyCode.VK_3;
                        break;
                    case '$':
                        keyCodeKey = VirtualKeyCode.VK_4;
                        break;
                    case '€':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.VK_5;
                        break;
                    case ';':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_COMMA;
                        break;
                    case ':':
                        shiftNeeded = true;
                        keyCodeKey = VirtualKeyCode.OEM_PERIOD;
                        break;
                    case '.':
                        keyCodeKey = VirtualKeyCode.OEM_PERIOD;
                        break;
                    case ',':
                        keyCodeKey = VirtualKeyCode.OEM_COMMA;
                        break;
                    case '_':
                        shiftNeeded=true;
                        keyCodeKey = VirtualKeyCode.OEM_MINUS;
                        break;
                    case '-':
                        keyCodeKey = VirtualKeyCode.OEM_MINUS;
                        break;
                    // Add more cases as needed for other characters specific to Danish keyboard
                    default:
                        // Fallback for letters, digits, and unhandled characters
                        // Implementation for ConvertToVirtualKeyCode not shown
                        var converted = ConvertToVirtualKeyCode(c);
                        shiftNeeded = char.IsUpper(c);
                        if (converted.HasValue)
                        {
                            keyCodeKey = converted.Value;
                        }
                        else
                        {
                            // Directly append unhandled characters
                            // Ensure richTextBox1 or similar output is available and correctly handled
                            if (HideText.Checked)
                            {
                                richTextBox1.AppendText("*");
                            }
                            else
                            {
                                richTextBox1.AppendText(c.ToString());
                            }
                        }
                        break;
                }

            }

            if (shiftNeeded)
            {
                inputSimulator.Keyboard.KeyDown(GregsStack.InputSimulatorStandard.Native.VirtualKeyCode.SHIFT);
                inputSimulator.Keyboard.KeyPress(keyCodeKey);
                inputSimulator.Keyboard.KeyUp(GregsStack.InputSimulatorStandard.Native.VirtualKeyCode.SHIFT);
            }
            else
            {
                inputSimulator.Keyboard.KeyPress(keyCodeKey);
            }
        }

        private GregsStack.InputSimulatorStandard.Native.VirtualKeyCode? ConvertToVirtualKeyCode(char c)
        {
            if (char.IsLetter(c))
            {
                // Check if the character is uppercase to adjust the keyCode accordingly
                bool isUpperCase = char.IsUpper(c);
                int keyCodeOffset = isUpperCase ? 'A' : 'a';
                int keyCode = c - keyCodeOffset + 0x41; // 0x41 is VirtualKeyCode for 'A'
                return (GregsStack.InputSimulatorStandard.Native.VirtualKeyCode)keyCode;
            }
            else if (char.IsDigit(c))
            {
                if (c == '0')
                {
                    return GregsStack.InputSimulatorStandard.Native.VirtualKeyCode.VK_0;
                }
                else
                {
                    int keyCode = c - '0' + 0x30; // 0x30 is VirtualKeyCode for '0'
                    return (GregsStack.InputSimulatorStandard.Native.VirtualKeyCode)keyCode;
                }
            }

            // Return null for characters that are not handled
            return null;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _hotKeyManager.Unregister();
            base.OnFormClosed(e);
        }

        private void EngKey_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
