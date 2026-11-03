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
            HideText = new CheckBox();
            LayoutBox = new ComboBox();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.Location = new Point(7, 6);
            richTextBox1.Margin = new Padding(2);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(443, 210);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // HideText
            // 
            HideText.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            HideText.AutoSize = true;
            HideText.Checked = true;
            HideText.CheckState = CheckState.Checked;
            HideText.Location = new Point(440, 195);
            HideText.Margin = new Padding(2);
            HideText.Name = "HideText";
            HideText.Size = new Size(89, 19);
            HideText.TabIndex = 1;
            HideText.Text = "Text Hidden";
            HideText.UseVisualStyleBackColor = true;
            // 
            // LayoutBox
            // 
            LayoutBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LayoutBox.DropDownStyle = ComboBoxStyle.DropDownList;
            LayoutBox.Location = new Point(420, 165);
            LayoutBox.Name = "LayoutBox";
            LayoutBox.Size = new Size(112, 23);
            LayoutBox.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(536, 225);
            Controls.Add(LayoutBox);
            Controls.Add(HideText);
            Controls.Add(richTextBox1);
            Margin = new Padding(2);
            Name = "Form1";
            Text = "KeyPuncher";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox richTextBox1;
        private CheckBox HideText;
        private ComboBox LayoutBox;
    }
}
