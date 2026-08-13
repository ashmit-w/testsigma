using System.Drawing;
using System.Windows.Forms;

namespace LegacyForms
{
    // Purely visual, non-interactive custom control: draws a simple
    // vertical bar chart with axis lines from 4 hardcoded values.
    public class BarChartPanel : Panel
    {
        private readonly int[] values = { 40, 75, 55, 90 };
        private readonly string[] labels = { "Q1", "Q2", "Q3", "Q4" };
        private const int MaxValue = 100;

        public BarChartPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.FillRectangle(SystemBrushes.Window, ClientRectangle);

            int margin = 24;
            int chartHeight = Height - margin * 2;
            int chartWidth = Width - margin * 2;
            int axisLeft = margin;
            int axisBottom = Height - margin;

            g.DrawLine(Pens.Black, axisLeft, margin, axisLeft, axisBottom);
            g.DrawLine(Pens.Black, axisLeft, axisBottom, axisLeft + chartWidth, axisBottom);

            int barCount = values.Length;
            int barWidth = chartWidth / (barCount * 2);

            for (int i = 0; i < barCount; i++)
            {
                int barHeight = (int)(values[i] / (float)MaxValue * chartHeight);
                int x = axisLeft + 10 + i * (barWidth + 20);
                int y = axisBottom - barHeight;

                g.FillRectangle(Brushes.SteelBlue, x, y, barWidth, barHeight);
                g.DrawRectangle(Pens.Black, x, y, barWidth, barHeight);
                g.DrawString(labels[i], Font, Brushes.Black, x, axisBottom + 2);
            }
        }
    }
}
