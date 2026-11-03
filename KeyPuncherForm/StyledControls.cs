using System.Drawing.Drawing2D;

namespace KeyPuncherForm
{
    internal static class Theme
    {
        public static readonly Color Background = Color.FromArgb(18, 21, 28);
        public static readonly Color Surface = Color.FromArgb(26, 30, 40);
        public static readonly Color Border = Color.FromArgb(52, 60, 78);
        public static readonly Color Text = Color.FromArgb(230, 233, 240);
        public static readonly Color Muted = Color.FromArgb(138, 147, 166);
        public static readonly Color Amber = Color.FromArgb(255, 181, 71);
        public static readonly Color Mint = Color.FromArgb(94, 234, 176);
        public static readonly Color Coral = Color.FromArgb(255, 122, 107);
    }

    /// <summary>A strip of punched-card holes that doubles as the typing progress bar.</summary>
    internal sealed class PunchStrip : Control
    {
        private const int Rows = 3;
        private float _progress;

        public PunchStrip()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Theme.Background;
        }

        /// <summary>How much of the card is punched, from 0 to 1.</summary>
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                Refresh(); // Typing blocks the message loop, so a plain Invalidate would never paint.
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int holeWidth = LogicalToDeviceUnits(4);
            int holeHeight = LogicalToDeviceUnits(7);
            int gapX = LogicalToDeviceUnits(4);
            int gapY = LogicalToDeviceUnits(3);
            int margin = LogicalToDeviceUnits(18);

            int columns = (Width - (2 * margin) + gapX) / (holeWidth + gapX);
            int litColumns = (int)(columns * Progress);

            using var unpunched = new SolidBrush(Color.FromArgb(38, 44, 58));
            using var punched = new SolidBrush(Theme.Amber);
            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    if (IsHole(column, row))
                    {
                        var hole = new Rectangle(margin + (column * (holeWidth + gapX)), row * (holeHeight + gapY), holeWidth, holeHeight);
                        e.Graphics.FillRectangle(column < litColumns ? punched : unpunched, hole);
                    }
                }
            }
        }

        // A fixed scramble, so the pattern looks like a card's data but never changes between paints.
        private static bool IsHole(int column, int row) => (((uint)((column * 31) + (row * 17)) * 2654435761u) >> 28) % 3 != 0;
    }

    /// <summary>A checkbox drawn as a toggle switch.</summary>
    internal sealed class ToggleSwitch : CheckBox
    {
        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Theme.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int trackWidth = LogicalToDeviceUnits(34);
            int trackHeight = LogicalToDeviceUnits(18);
            int top = (Height - trackHeight) / 2;
            var track = new Rectangle(0, top, trackWidth, trackHeight);

            using (GraphicsPath path = Pill(track))
            using (var brush = new SolidBrush(Checked ? Theme.Amber : Theme.Border))
            {
                g.FillPath(brush, path);
            }

            int inset = LogicalToDeviceUnits(3);
            int knobSize = trackHeight - (2 * inset);
            int knobX = Checked ? trackWidth - knobSize - inset : inset;
            using (var knob = new SolidBrush(Checked ? Theme.Background : Theme.Text))
            {
                g.FillEllipse(knob, knobX, top + inset, knobSize, knobSize);
            }

            var textArea = new Rectangle(trackWidth + LogicalToDeviceUnits(8), 0, Width - trackWidth - LogicalToDeviceUnits(8), Height);
            TextRenderer.DrawText(g, Text, Font, textArea, Focused ? Theme.Text : Theme.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }

        private static GraphicsPath Pill(Rectangle r)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, r.Height, r.Height, 90, 180);
            path.AddArc(r.Right - r.Height, r.Y, r.Height, r.Height, 270, 180);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>A dropdown that stays dark, where the default draws its selected item in the system highlight colour.</summary>
    internal sealed class ThemedComboBox : ComboBox
    {
        public ThemedComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            BackColor = Theme.Surface;
            ForeColor = Theme.Text;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_PAINT = 0x000F;
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT)
            {
                PaintFrameAndArrow();
            }
        }

        // Windows paints these parts of a flat combo box in bright colours whatever BackColor is set to.
        private void PaintFrameAndArrow()
        {
            using Graphics g = Graphics.FromHwnd(Handle);
            int arrowWidth = SystemInformation.VerticalScrollBarWidth + 2;
            var arrowArea = new Rectangle(Width - arrowWidth, 1, arrowWidth - 1, Height - 2);
            using (var back = new SolidBrush(Theme.Surface))
            {
                g.FillRectangle(back, arrowArea);
            }

            using (var border = new Pen(Theme.Border))
            {
                g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
            }

            int size = LogicalToDeviceUnits(4);
            int centerX = arrowArea.Left + (arrowArea.Width / 2);
            int centerY = Height / 2;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var chevron = new Pen(Theme.Muted, LogicalToDeviceUnits(2) * 0.75f);
            g.DrawLines(chevron, new[]
            {
                new Point(centerX - size, centerY - (size / 2)),
                new Point(centerX, centerY + (size / 2)),
                new Point(centerX + size, centerY - (size / 2)),
            });
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            bool hovered = e.State.HasFlag(DrawItemState.Selected) && !e.State.HasFlag(DrawItemState.ComboBoxEdit);
            using var back = new SolidBrush(hovered ? Theme.Border : Theme.Surface);
            e.Graphics.FillRectangle(back, e.Bounds);
            TextRenderer.DrawText(e.Graphics, Items[e.Index].ToString(), Font, e.Bounds, Theme.Text, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
    }
}
