namespace KeyPuncherForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            richTextBox1 = new RichTextBox();
            HideText = new ToggleSwitch();
            LayoutBox = new ThemedComboBox();
            Header = new Panel();
            TitleLabel = new Label();
            HintLabel = new Label();
            StatusLabel = new Label();
            Strip = new PunchStrip();
            PreviewFrame = new Panel();
            PreviewSurface = new Panel();
            Footer = new Panel();
            LayoutLabel = new Label();
            Header.SuspendLayout();
            PreviewFrame.SuspendLayout();
            PreviewSurface.SuspendLayout();
            Footer.SuspendLayout();
            SuspendLayout();
            //
            // Header
            //
            Header.Controls.Add(StatusLabel);
            Header.Controls.Add(HintLabel);
            Header.Controls.Add(TitleLabel);
            Header.Dock = DockStyle.Top;
            Header.Name = "Header";
            Header.Size = new Size(620, 76);
            Header.TabIndex = 0;
            //
            // TitleLabel
            //
            TitleLabel.AutoSize = true;
            TitleLabel.Font = new Font("Segoe UI Semibold", 16F);
            TitleLabel.ForeColor = Theme.Text;
            TitleLabel.Location = new Point(14, 8);
            TitleLabel.Name = "TitleLabel";
            TitleLabel.Text = "KeyPuncher";
            //
            // HintLabel
            //
            HintLabel.AutoSize = true;
            HintLabel.ForeColor = Theme.Muted;
            HintLabel.Location = new Point(18, 46);
            HintLabel.Name = "HintLabel";
            HintLabel.Text = "Copy your text, click the remote window, press Ctrl + Alt + V";
            //
            // StatusLabel
            //
            StatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            StatusLabel.Font = new Font("Segoe UI Semibold", 10F);
            StatusLabel.ForeColor = Theme.Mint;
            StatusLabel.Location = new Point(382, 14);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new Size(220, 24);
            StatusLabel.Text = "●  Ready";
            StatusLabel.TextAlign = ContentAlignment.MiddleRight;
            //
            // Strip
            //
            Strip.Dock = DockStyle.Top;
            Strip.Name = "Strip";
            Strip.Size = new Size(620, 36);
            Strip.TabStop = false;
            //
            // PreviewFrame
            //
            PreviewFrame.Controls.Add(PreviewSurface);
            PreviewFrame.Dock = DockStyle.Fill;
            PreviewFrame.Name = "PreviewFrame";
            PreviewFrame.Padding = new Padding(18, 0, 18, 0);
            PreviewFrame.TabIndex = 1;
            //
            // PreviewSurface
            //
            PreviewSurface.BackColor = Theme.Surface;
            PreviewSurface.Controls.Add(richTextBox1);
            PreviewSurface.Dock = DockStyle.Fill;
            PreviewSurface.Name = "PreviewSurface";
            PreviewSurface.Padding = new Padding(12, 10, 12, 10);
            PreviewSurface.TabIndex = 0;
            //
            // richTextBox1
            //
            richTextBox1.BackColor = Theme.Surface;
            richTextBox1.BorderStyle = BorderStyle.None;
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Font = new Font("Consolas", 10F);
            richTextBox1.ForeColor = Theme.Muted;
            richTextBox1.Name = "richTextBox1";
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            //
            // Footer
            //
            Footer.Controls.Add(HideText);
            Footer.Controls.Add(LayoutBox);
            Footer.Controls.Add(LayoutLabel);
            Footer.Dock = DockStyle.Bottom;
            Footer.Name = "Footer";
            Footer.Size = new Size(620, 56);
            Footer.TabIndex = 2;
            //
            // LayoutLabel
            //
            LayoutLabel.AutoSize = true;
            LayoutLabel.ForeColor = Theme.Muted;
            LayoutLabel.Location = new Point(18, 19);
            LayoutLabel.Name = "LayoutLabel";
            LayoutLabel.Text = "Remote keyboard";
            //
            // LayoutBox
            //
            LayoutBox.Location = new Point(128, 15);
            LayoutBox.Name = "LayoutBox";
            LayoutBox.Size = new Size(150, 24);
            LayoutBox.TabIndex = 0;
            //
            // HideText
            //
            HideText.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            HideText.Checked = true;
            HideText.CheckState = CheckState.Checked;
            HideText.Location = new Point(490, 14);
            HideText.Name = "HideText";
            HideText.Size = new Size(112, 26);
            HideText.TabIndex = 1;
            HideText.Text = "Hide text";
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Theme.Background;
            ClientSize = new Size(620, 340);
            Controls.Add(PreviewFrame);
            Controls.Add(Strip);
            Controls.Add(Footer);
            Controls.Add(Header);
            ForeColor = Theme.Text;
            Margin = new Padding(2);
            MinimumSize = new Size(560, 320);
            Name = "Form1";
            Text = "KeyPuncher";
            Load += Form1_Load;
            Header.ResumeLayout(false);
            Header.PerformLayout();
            PreviewFrame.ResumeLayout(false);
            PreviewSurface.ResumeLayout(false);
            Footer.ResumeLayout(false);
            Footer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox richTextBox1;
        private ToggleSwitch HideText;
        private ThemedComboBox LayoutBox;
        private Panel Header;
        private Label TitleLabel;
        private Label HintLabel;
        private Label StatusLabel;
        private PunchStrip Strip;
        private Panel PreviewFrame;
        private Panel PreviewSurface;
        private Panel Footer;
        private Label LayoutLabel;
    }
}
