using System;
using System.Drawing;
using System.Windows.Forms;

namespace LegacyForms
{
    // Modal "About" dialog. Deliberately uses designer-default control
    // names only; no AccessibleName / AccessibleDescription is set anywhere
    // in this application.
    public class AboutForm : Form
    {
        public AboutForm()
        {
            Name = "AboutForm";
            Text = "About LegacyForms";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(320, 150);

            Label label1 = new Label();
            label1.Name = "label1";
            label1.Text = "LegacyForms" + Environment.NewLine +
                           "A Windows XP era line-of-business test application." + Environment.NewLine +
                           "Version 1.0";
            label1.Location = new Point(20, 20);
            label1.Size = new Size(280, 70);
            Controls.Add(label1);

            Button button1 = new Button();
            button1.Name = "button1";
            button1.Text = "OK";
            button1.Location = new Point(120, 100);
            button1.Size = new Size(80, 26);
            button1.DialogResult = DialogResult.OK;
            Controls.Add(button1);

            AcceptButton = button1;
        }
    }
}
