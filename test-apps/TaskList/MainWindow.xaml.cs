using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace TaskListApp
{
    public class TaskItem
    {
        public string Text;
        public bool Done;
    }

    public partial class MainWindow : Window
    {
        private readonly List<TaskItem> tasks = new List<TaskItem>
        {
            new TaskItem { Text = "Buy groceries", Done = false },
            new TaskItem { Text = "Write report", Done = true },
            new TaskItem { Text = "Call plumber", Done = false },
        };

        public MainWindow()
        {
            InitializeComponent();
            RefreshList();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            string text = textBoxNewTask.Text.Trim();
            if (text.Length == 0)
            {
                return;
            }

            tasks.Add(new TaskItem { Text = text, Done = false });
            textBoxNewTask.Text = string.Empty;
            RefreshList();
        }

        private void ButtonDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            List<int> selectedIndexes = listBoxTasks.SelectedItems
                .Cast<CheckBox>()
                .Select(checkBox => (int)checkBox.Tag)
                .OrderByDescending(index => index)
                .ToList();

            foreach (int index in selectedIndexes)
            {
                tasks.RemoveAt(index);
            }

            RefreshList();
        }

        private void TaskCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            int index = (int)checkBox.Tag;
            tasks[index].Done = checkBox.IsChecked == true;
            UpdateSummary();
        }

        private void RefreshList()
        {
            listBoxTasks.Items.Clear();

            for (int i = 0; i < tasks.Count; i++)
            {
                CheckBox checkBox = new CheckBox
                {
                    Content = tasks[i].Text,
                    IsChecked = tasks[i].Done,
                    Tag = i,
                };
                AutomationProperties.SetAutomationId(checkBox, "checkBoxTask" + i);
                checkBox.Checked += TaskCheckBox_Changed;
                checkBox.Unchecked += TaskCheckBox_Changed;

                listBoxTasks.Items.Add(checkBox);
            }

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            int complete = tasks.Count(t => t.Done);
            textBlockSummary.Text = complete + " of " + tasks.Count + " complete";
        }
    }
}
