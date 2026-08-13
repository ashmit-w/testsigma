using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SimpleCalc
{
    // A clean, ordinary WinForms app - in contrast to LegacyForms every
    // button here has a descriptive Name and an explicit AccessibleName.
    public class CalcForm : Form
    {
        private TextBox textBoxDisplay;

        private string currentEntry = "0";
        private double storedValue;
        private string pendingOperator;
        private bool startNewEntry = true;
        private bool errorState;

        public CalcForm()
        {
            Name = "CalcForm";
            Text = "SimpleCalc";
            ClientSize = new Size(300, 400);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;

            BuildDisplay();
            BuildButtons();
            UpdateDisplay();
        }

        private void BuildDisplay()
        {
            textBoxDisplay = new TextBox();
            textBoxDisplay.Name = "textBoxDisplay";
            textBoxDisplay.AccessibleName = "Display";
            textBoxDisplay.Location = new Point(10, 10);
            textBoxDisplay.Size = new Size(280, 40);
            textBoxDisplay.ReadOnly = true;
            textBoxDisplay.TextAlign = HorizontalAlignment.Right;
            textBoxDisplay.TabStop = false;
            textBoxDisplay.Font = new Font(Font.FontFamily, 18f, FontStyle.Regular);
            Controls.Add(textBoxDisplay);
        }

        private void BuildButtons()
        {
            AddButton("btnSeven", "7", "Seven", 10, 70, ButtonDigit_Click, "7");
            AddButton("btnEight", "8", "Eight", 80, 70, ButtonDigit_Click, "8");
            AddButton("btnNine", "9", "Nine", 150, 70, ButtonDigit_Click, "9");
            AddButton("btnDivide", "/", "Divide", 220, 70, ButtonOperator_Click, "/");

            AddButton("btnFour", "4", "Four", 10, 130, ButtonDigit_Click, "4");
            AddButton("btnFive", "5", "Five", 80, 130, ButtonDigit_Click, "5");
            AddButton("btnSix", "6", "Six", 150, 130, ButtonDigit_Click, "6");
            AddButton("btnMultiply", "*", "Multiply", 220, 130, ButtonOperator_Click, "*");

            AddButton("btnOne", "1", "One", 10, 190, ButtonDigit_Click, "1");
            AddButton("btnTwo", "2", "Two", 80, 190, ButtonDigit_Click, "2");
            AddButton("btnThree", "3", "Three", 150, 190, ButtonDigit_Click, "3");
            AddButton("btnSubtract", "-", "Subtract", 220, 190, ButtonOperator_Click, "-");

            AddButton("btnClear", "C", "Clear", 10, 250, (s, e) => ClearAll(), null);
            AddButton("btnZero", "0", "Zero", 80, 250, ButtonDigit_Click, "0");
            AddButton("btnDecimal", ".", "Decimal Point", 150, 250, (s, e) => ButtonDecimal_Click(), null);
            AddButton("btnAdd", "+", "Add", 220, 250, ButtonOperator_Click, "+");

            Button btnEquals = AddButton("btnEquals", "=", "Equals", 10, 310, (s, e) => ButtonEquals_Click(), null);
            btnEquals.Size = new Size(270, 50);
        }

        private Button AddButton(string name, string text, string accessibleName, int x, int y,
            EventHandler handler, string tag)
        {
            Button button = new Button();
            button.Name = name;
            button.Text = text;
            button.AccessibleName = accessibleName;
            button.Location = new Point(x, y);
            button.Size = new Size(60, 50);
            button.Font = new Font(Font.FontFamily, 12f, FontStyle.Regular);
            button.Tag = tag;
            button.Click += handler;
            Controls.Add(button);
            return button;
        }

        private void ButtonDigit_Click(object sender, EventArgs e)
        {
            string digit = (string)((Button)sender).Tag;

            if (errorState)
            {
                ClearAll();
            }

            if (startNewEntry || currentEntry == "0")
            {
                currentEntry = digit;
                startNewEntry = false;
            }
            else
            {
                currentEntry += digit;
            }

            UpdateDisplay();
        }

        private void ButtonDecimal_Click()
        {
            if (errorState)
            {
                ClearAll();
            }

            if (startNewEntry)
            {
                currentEntry = "0.";
                startNewEntry = false;
            }
            else if (currentEntry.IndexOf('.') < 0)
            {
                currentEntry += ".";
            }

            UpdateDisplay();
        }

        private void ButtonOperator_Click(object sender, EventArgs e)
        {
            string op = (string)((Button)sender).Tag;

            if (errorState)
            {
                return;
            }

            if (pendingOperator != null && !startNewEntry)
            {
                if (!TryCompute(out double result))
                {
                    ShowError();
                    return;
                }
                storedValue = result;
                currentEntry = FormatNumber(result);
            }
            else
            {
                storedValue = double.Parse(currentEntry, CultureInfo.InvariantCulture);
            }

            pendingOperator = op;
            startNewEntry = true;
            UpdateDisplay();
        }

        private void ButtonEquals_Click()
        {
            if (errorState || pendingOperator == null)
            {
                return;
            }

            if (!TryCompute(out double result))
            {
                ShowError();
                return;
            }

            currentEntry = FormatNumber(result);
            storedValue = result;
            pendingOperator = null;
            startNewEntry = true;
            UpdateDisplay();
        }

        private bool TryCompute(out double result)
        {
            double current = double.Parse(currentEntry, CultureInfo.InvariantCulture);
            switch (pendingOperator)
            {
                case "+":
                    result = storedValue + current;
                    return true;
                case "-":
                    result = storedValue - current;
                    return true;
                case "*":
                    result = storedValue * current;
                    return true;
                case "/":
                    if (current == 0)
                    {
                        result = 0;
                        return false;
                    }
                    result = storedValue / current;
                    return true;
                default:
                    result = current;
                    return true;
            }
        }

        private void ShowError()
        {
            errorState = true;
            pendingOperator = null;
            startNewEntry = true;
            UpdateDisplay();
        }

        private void ClearAll()
        {
            currentEntry = "0";
            storedValue = 0;
            pendingOperator = null;
            startNewEntry = true;
            errorState = false;
            UpdateDisplay();
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("G15", CultureInfo.InvariantCulture);
        }

        private void UpdateDisplay()
        {
            textBoxDisplay.Text = errorState ? "Error" : currentEntry;
        }
    }
}
