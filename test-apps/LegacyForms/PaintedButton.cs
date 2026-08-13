using System;
using System.Drawing;
using System.Windows.Forms;

namespace LegacyForms
{
    // A custom-painted, non-standard "button" built on top of Panel.
    // Contains no child controls and sets no accessibility properties -
    // by design it exercises the "unlabeled custom control" case.
    public class PaintedButton : Panel
    {
        private const string ButtonText = "Save Draft";

        public event EventHandler DraftSaved;

        public PaintedButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            MouseClick += PaintedButton_MouseClick;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            e.Graphics.FillRectangle(SystemBrushes.ControlLight, rect);
            e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, rect);

            SizeF textSize = e.Graphics.MeasureString(ButtonText, Font);
            PointF location = new PointF((Width - textSize.Width) / 2f, (Height - textSize.Height) / 2f);
            e.Graphics.DrawString(ButtonText, Font, SystemBrushes.ControlText, location);
        }

        private void PaintedButton_MouseClick(object sender, MouseEventArgs e)
        {
            DraftSaved?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }
}
