using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LegacyForms
{
    // Windows XP era line-of-business form. Every control is left at its
    // designer-default Name (textBox1, button3, listBox1, ...) and no
    // AccessibleName / AccessibleDescription is ever set. This is
    // intentional: this app is the "hostile to accessibility APIs" target.
    public class MainForm : Form
    {
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem addRecordToolStripMenuItem;
        private ToolStripMenuItem updateRecordToolStripMenuItem;
        private ToolStripMenuItem deleteRecordToolStripMenuItem;
        private ToolStripMenuItem clearFieldsToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem showChartToolStripMenuItem;
        private ToolStripMenuItem showStatusBarToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;

        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripButton toolStripButton4;

        private Panel contentPanel;

        private GroupBox groupBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private TextBox textBox1;
        private TextBox textBox2;
        private TextBox textBox3;
        private ComboBox comboBox1;
        private DateTimePicker dateTimePicker1;
        private CheckBox checkBox1;
        private GroupBox groupBox2;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private NumericUpDown numericUpDown1;

        private ListBox listBox1;

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TextBox textBox4;
        private ListView listView1;

        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;

        private PaintedButton panel1;
        private BarChartPanel panel2;

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;

        private List<Employee> employees;

        public MainForm()
        {
            Name = "MainForm";
            Text = "LegacyForms";
            Size = new Size(900, 620);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            Font = SystemFonts.DefaultFont;

            employees = BuildEmployeeData();

            BuildMenu();
            BuildToolStrip();
            BuildContent();
            BuildStatusStrip();

            Controls.Add(contentPanel);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;

            PopulateEmployeeList();
            UpdateRecordCount();
            SetStatus("Ready.");
        }

        private static List<Employee> BuildEmployeeData()
        {
            return new List<Employee>
            {
                new Employee("E001", "John Smith", "IT", "Active", new DateTime(2018, 3, 15), true, "Day", 6),
                new Employee("E002", "Mary Johnson", "HR", "Active", new DateTime(2019, 7, 22), true, "Day", 5),
                new Employee("E003", "Robert Brown", "Finance", "On Leave", new DateTime(2017, 1, 10), true, "Night", 7),
                new Employee("E004", "Patricia Davis", "IT", "Active", new DateTime(2020, 11, 5), false, "Day", 3),
                new Employee("E005", "Michael Wilson", "Operations", "Terminated", new DateTime(2015, 6, 30), true, "Night", 8),
                new Employee("E006", "Linda Miller", "Sales", "Active", new DateTime(2021, 2, 18), true, "Day", 4),
                new Employee("E007", "James Moore", "IT", "Active", new DateTime(2016, 9, 9), false, "Night", 9),
                new Employee("E008", "Barbara Taylor", "HR", "On Leave", new DateTime(2019, 12, 1), true, "Day", 5),
            };
        }

        private void BuildMenu()
        {
            menuStrip1 = new MenuStrip();
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Dock = DockStyle.Top;

            fileToolStripMenuItem = new ToolStripMenuItem("File");
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            newToolStripMenuItem = new ToolStripMenuItem("New");
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.Click += (s, e) => { ClearFields(); SetStatus("New record started."); };
            openToolStripMenuItem = new ToolStripMenuItem("Open");
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Click += (s, e) => SetStatus("Open is not implemented in this demo.");
            saveToolStripMenuItem = new ToolStripMenuItem("Save");
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Click += (s, e) => SaveRecord();
            exitToolStripMenuItem = new ToolStripMenuItem("Exit");
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Click += (s, e) => Close();
            fileToolStripMenuItem.DropDownItems.Add(newToolStripMenuItem);
            fileToolStripMenuItem.DropDownItems.Add(openToolStripMenuItem);
            fileToolStripMenuItem.DropDownItems.Add(saveToolStripMenuItem);
            fileToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            fileToolStripMenuItem.DropDownItems.Add(exitToolStripMenuItem);

            editToolStripMenuItem = new ToolStripMenuItem("Edit");
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            addRecordToolStripMenuItem = new ToolStripMenuItem("Add Record");
            addRecordToolStripMenuItem.Name = "addRecordToolStripMenuItem";
            addRecordToolStripMenuItem.Click += (s, e) => AddRecord();
            updateRecordToolStripMenuItem = new ToolStripMenuItem("Update Record");
            updateRecordToolStripMenuItem.Name = "updateRecordToolStripMenuItem";
            updateRecordToolStripMenuItem.Enabled = false;
            updateRecordToolStripMenuItem.Click += (s, e) => UpdateRecord();
            deleteRecordToolStripMenuItem = new ToolStripMenuItem("Delete Record");
            deleteRecordToolStripMenuItem.Name = "deleteRecordToolStripMenuItem";
            deleteRecordToolStripMenuItem.Click += (s, e) => DeleteRecord();
            clearFieldsToolStripMenuItem = new ToolStripMenuItem("Clear Fields");
            clearFieldsToolStripMenuItem.Name = "clearFieldsToolStripMenuItem";
            clearFieldsToolStripMenuItem.Click += (s, e) => ClearFields();
            editToolStripMenuItem.DropDownItems.Add(addRecordToolStripMenuItem);
            editToolStripMenuItem.DropDownItems.Add(updateRecordToolStripMenuItem);
            editToolStripMenuItem.DropDownItems.Add(deleteRecordToolStripMenuItem);
            editToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            editToolStripMenuItem.DropDownItems.Add(clearFieldsToolStripMenuItem);

            viewToolStripMenuItem = new ToolStripMenuItem("View");
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            showChartToolStripMenuItem = new ToolStripMenuItem("Show Chart");
            showChartToolStripMenuItem.Name = "showChartToolStripMenuItem";
            showChartToolStripMenuItem.CheckOnClick = true;
            showChartToolStripMenuItem.Checked = true;
            showChartToolStripMenuItem.Click += (s, e) => { panel2.Visible = showChartToolStripMenuItem.Checked; };
            showStatusBarToolStripMenuItem = new ToolStripMenuItem("Show Status Bar");
            showStatusBarToolStripMenuItem.Name = "showStatusBarToolStripMenuItem";
            showStatusBarToolStripMenuItem.CheckOnClick = true;
            showStatusBarToolStripMenuItem.Checked = true;
            showStatusBarToolStripMenuItem.Click += (s, e) => { statusStrip1.Visible = showStatusBarToolStripMenuItem.Checked; };
            viewToolStripMenuItem.DropDownItems.Add(showChartToolStripMenuItem);
            viewToolStripMenuItem.DropDownItems.Add(showStatusBarToolStripMenuItem);

            helpToolStripMenuItem = new ToolStripMenuItem("Help");
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            aboutToolStripMenuItem = new ToolStripMenuItem("About");
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Click += (s, e) =>
            {
                using (AboutForm aboutForm = new AboutForm())
                {
                    aboutForm.ShowDialog(this);
                }
            };
            helpToolStripMenuItem.DropDownItems.Add(aboutToolStripMenuItem);

            menuStrip1.Items.Add(fileToolStripMenuItem);
            menuStrip1.Items.Add(editToolStripMenuItem);
            menuStrip1.Items.Add(viewToolStripMenuItem);
            menuStrip1.Items.Add(helpToolStripMenuItem);
        }

        private void BuildToolStrip()
        {
            toolStrip1 = new ToolStrip();
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Dock = DockStyle.Top;

            toolStripButton1 = new ToolStripButton("New");
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.Click += (s, e) => { ClearFields(); SetStatus("New record started."); };

            toolStripButton2 = new ToolStripButton("Save");
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.Click += (s, e) => SaveRecord();

            toolStripButton3 = new ToolStripButton("Delete");
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.Click += (s, e) => DeleteRecord();

            toolStripButton4 = new ToolStripButton("Refresh");
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton4.Click += (s, e) =>
            {
                listBox1.ClearSelected();
                ClearFields();
                SetStatus("View refreshed.");
            };

            toolStrip1.Items.Add(toolStripButton1);
            toolStrip1.Items.Add(toolStripButton2);
            toolStrip1.Items.Add(toolStripButton3);
            toolStrip1.Items.Add(toolStripButton4);
        }

        private void BuildContent()
        {
            contentPanel = new Panel();
            contentPanel.Name = "contentPanel";
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.AutoScroll = true;

            BuildEmployeeDetailsGroup();
            BuildPaintedControls();
            BuildEmployeeList();
            BuildTabs();
            BuildActionButtons();

            contentPanel.Controls.Add(groupBox1);
            contentPanel.Controls.Add(panel1);
            contentPanel.Controls.Add(panel2);
            contentPanel.Controls.Add(listBox1);
            contentPanel.Controls.Add(tabControl1);
            contentPanel.Controls.Add(button1);
            contentPanel.Controls.Add(button2);
            contentPanel.Controls.Add(button3);
            contentPanel.Controls.Add(button4);
        }

        private void BuildEmployeeDetailsGroup()
        {
            groupBox1 = new GroupBox();
            groupBox1.Name = "groupBox1";
            groupBox1.Text = "Employee Details";
            groupBox1.Location = new Point(12, 10);
            groupBox1.Size = new Size(300, 350);

            label1 = new Label { Name = "label1", Text = "Name:", Location = new Point(10, 25), Size = new Size(90, 20) };
            textBox1 = new TextBox { Name = "textBox1", Location = new Point(110, 22), Size = new Size(170, 20) };

            label2 = new Label { Name = "label2", Text = "Employee ID:", Location = new Point(10, 55), Size = new Size(90, 20) };
            textBox2 = new TextBox { Name = "textBox2", Location = new Point(110, 52), Size = new Size(170, 20) };

            label3 = new Label { Name = "label3", Text = "Department:", Location = new Point(10, 85), Size = new Size(90, 20) };
            textBox3 = new TextBox { Name = "textBox3", Location = new Point(110, 82), Size = new Size(170, 20) };

            label4 = new Label { Name = "label4", Text = "Status:", Location = new Point(10, 115), Size = new Size(90, 20) };
            comboBox1 = new ComboBox { Name = "comboBox1", Location = new Point(110, 112), Size = new Size(170, 21), DropDownStyle = ComboBoxStyle.DropDownList };
            comboBox1.Items.AddRange(new object[] { "Active", "On Leave", "Terminated" });
            comboBox1.SelectedIndex = 0;

            label5 = new Label { Name = "label5", Text = "Join Date:", Location = new Point(10, 145), Size = new Size(90, 20) };
            dateTimePicker1 = new DateTimePicker { Name = "dateTimePicker1", Location = new Point(110, 142), Size = new Size(170, 20), Format = DateTimePickerFormat.Short };

            checkBox1 = new CheckBox { Name = "checkBox1", Text = "Full Time", Location = new Point(110, 172), Size = new Size(150, 20) };

            groupBox2 = new GroupBox { Name = "groupBox2", Text = "Shift", Location = new Point(10, 200), Size = new Size(270, 55) };
            radioButton1 = new RadioButton { Name = "radioButton1", Text = "Day", Location = new Point(15, 20), Size = new Size(100, 20), Checked = true };
            radioButton2 = new RadioButton { Name = "radioButton2", Text = "Night", Location = new Point(140, 20), Size = new Size(100, 20) };
            groupBox2.Controls.Add(radioButton1);
            groupBox2.Controls.Add(radioButton2);

            label6 = new Label { Name = "label6", Text = "Salary Band (1-12):", Location = new Point(10, 268), Size = new Size(150, 20) };
            numericUpDown1 = new NumericUpDown { Name = "numericUpDown1", Location = new Point(170, 265), Size = new Size(60, 20), Minimum = 1, Maximum = 12, Value = 1 };

            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(textBox2);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(textBox3);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(dateTimePicker1);
            groupBox1.Controls.Add(checkBox1);
            groupBox1.Controls.Add(groupBox2);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(numericUpDown1);
        }

        private void BuildPaintedControls()
        {
            panel1 = new PaintedButton();
            panel1.Name = "panel1";
            panel1.Location = new Point(12, 368);
            panel1.Size = new Size(300, 40);
            panel1.BorderStyle = BorderStyle.None;
            panel1.DraftSaved += (s, e) => SetStatus("Draft saved (unofficial).");

            panel2 = new BarChartPanel();
            panel2.Name = "panel2";
            panel2.Location = new Point(12, 414);
            panel2.Size = new Size(300, 150);
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Visible = true;
        }

        private void BuildEmployeeList()
        {
            listBox1 = new ListBox();
            listBox1.Name = "listBox1";
            listBox1.Location = new Point(324, 10);
            listBox1.Size = new Size(280, 180);
            listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
        }

        private void BuildTabs()
        {
            tabControl1 = new TabControl();
            tabControl1.Name = "tabControl1";
            tabControl1.Location = new Point(324, 198);
            tabControl1.Size = new Size(280, 180);

            tabPage1 = new TabPage("Notes");
            tabPage1.Name = "tabPage1";
            textBox4 = new TextBox();
            textBox4.Name = "textBox4";
            textBox4.Multiline = true;
            textBox4.ScrollBars = ScrollBars.Vertical;
            textBox4.Dock = DockStyle.Fill;
            tabPage1.Controls.Add(textBox4);

            tabPage2 = new TabPage("History");
            tabPage2.Name = "tabPage2";
            listView1 = new ListView();
            listView1.Name = "listView1";
            listView1.Dock = DockStyle.Fill;
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Columns.Add("Date", 80);
            listView1.Columns.Add("Action", 100);
            listView1.Columns.Add("User", 80);
            listView1.Items.Add(new ListViewItem(new[] { "2024-01-05", "Created", "admin" }));
            listView1.Items.Add(new ListViewItem(new[] { "2024-02-10", "Updated", "jsmith" }));
            listView1.Items.Add(new ListViewItem(new[] { "2024-03-22", "StatusChanged", "hr_admin" }));
            listView1.Items.Add(new ListViewItem(new[] { "2024-05-01", "Updated", "admin" }));
            listView1.Items.Add(new ListViewItem(new[] { "2024-06-15", "Reviewed", "manager1" }));
            tabPage2.Controls.Add(listView1);

            tabControl1.TabPages.Add(tabPage1);
            tabControl1.TabPages.Add(tabPage2);
        }

        private void BuildActionButtons()
        {
            button1 = new Button { Name = "button1", Text = "Add", Location = new Point(324, 390), Size = new Size(60, 30) };
            button1.Click += (s, e) => AddRecord();

            button2 = new Button { Name = "button2", Text = "Update", Location = new Point(392, 390), Size = new Size(60, 30), Enabled = false };
            button2.Click += (s, e) => UpdateRecord();

            button3 = new Button { Name = "button3", Text = "Delete", Location = new Point(460, 390), Size = new Size(60, 30) };
            button3.Click += (s, e) => DeleteRecord();

            button4 = new Button { Name = "button4", Text = "Clear", Location = new Point(528, 390), Size = new Size(60, 30) };
            button4.Click += (s, e) => ClearFields();
        }

        private void BuildStatusStrip()
        {
            statusStrip1 = new StatusStrip();
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Dock = DockStyle.Bottom;

            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Spring = true;
            toolStripStatusLabel1.TextAlign = ContentAlignment.MiddleLeft;

            toolStripStatusLabel2 = new ToolStripStatusLabel();
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";

            statusStrip1.Items.Add(toolStripStatusLabel1);
            statusStrip1.Items.Add(toolStripStatusLabel2);
        }

        private void PopulateEmployeeList()
        {
            listBox1.Items.Clear();
            foreach (Employee employee in employees)
            {
                listBox1.Items.Add(employee);
            }
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is Employee employee)
            {
                PopulateFieldsFromEmployee(employee);
                button2.Enabled = true;
                updateRecordToolStripMenuItem.Enabled = true;
                SetStatus("Selected " + employee.Name + ".");
            }
            else
            {
                button2.Enabled = false;
                updateRecordToolStripMenuItem.Enabled = false;
            }
        }

        private void PopulateFieldsFromEmployee(Employee employee)
        {
            textBox1.Text = employee.Name;
            textBox2.Text = employee.Id;
            textBox3.Text = employee.Department;
            comboBox1.SelectedItem = employee.Status;
            dateTimePicker1.Value = employee.JoinDate;
            checkBox1.Checked = employee.FullTime;
            radioButton1.Checked = employee.Shift == "Day";
            radioButton2.Checked = employee.Shift == "Night";
            numericUpDown1.Value = employee.SalaryBand;
        }

        private Employee ReadEmployeeFromFields()
        {
            return new Employee(
                textBox2.Text.Trim(),
                textBox1.Text.Trim(),
                textBox3.Text.Trim(),
                comboBox1.SelectedItem?.ToString() ?? "Active",
                dateTimePicker1.Value.Date,
                checkBox1.Checked,
                radioButton2.Checked ? "Night" : "Day",
                (int)numericUpDown1.Value);
        }

        private void SaveRecord()
        {
            if (listBox1.SelectedIndex >= 0)
            {
                UpdateRecord();
            }
            else
            {
                AddRecord();
            }
        }

        private void AddRecord()
        {
            if (textBox1.Text.Trim().Length == 0 || textBox2.Text.Trim().Length == 0)
            {
                MessageBox.Show(this, "Please enter at least Name and Employee ID.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Employee employee = ReadEmployeeFromFields();
            employees.Add(employee);
            listBox1.Items.Add(employee);
            listBox1.SelectedItem = employee;
            SetStatus("Record added.");
            UpdateRecordCount();
        }

        private void UpdateRecord()
        {
            int index = listBox1.SelectedIndex;
            if (index < 0)
            {
                MessageBox.Show(this, "Please select a record to update.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Employee employee = ReadEmployeeFromFields();
            employees[index] = employee;
            listBox1.Items[index] = employee;
            listBox1.SelectedIndex = index;
            SetStatus("Record updated.");
            UpdateRecordCount();
        }

        private void DeleteRecord()
        {
            int index = listBox1.SelectedIndex;
            if (index < 0)
            {
                MessageBox.Show(this, "Please select a record to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            employees.RemoveAt(index);
            listBox1.Items.RemoveAt(index);
            ClearFields();
            SetStatus("Record deleted.");
            UpdateRecordCount();
        }

        private void ClearFields()
        {
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
            textBox3.Text = string.Empty;
            comboBox1.SelectedIndex = 0;
            dateTimePicker1.Value = DateTime.Today;
            checkBox1.Checked = false;
            radioButton1.Checked = true;
            numericUpDown1.Value = numericUpDown1.Minimum;
            listBox1.ClearSelected();
            button2.Enabled = false;
            updateRecordToolStripMenuItem.Enabled = false;
            SetStatus("Fields cleared.");
        }

        private void SetStatus(string message)
        {
            toolStripStatusLabel1.Text = message;
        }

        private void UpdateRecordCount()
        {
            toolStripStatusLabel2.Text = "Records: " + listBox1.Items.Count;
        }
    }
}
