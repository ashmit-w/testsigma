using System;
using System.Windows;
using System.Windows.Controls;

namespace ControlZoo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Several controls below have their initial Value/SelectedIndex set
        // in XAML, which raises their change event synchronously while
        // InitializeComponent() is still connecting later-declared fields
        // (including statusText). Guard against firing before that field
        // is wired up.
        private bool IsReady => statusText != null;

        private void TextBoxSingleLine_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsReady) return;
            statusText.Text = "TextBox changed: " + textBoxSingleLine.Text;
        }

        private void PasswordBoxDemo_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!IsReady) return;
            statusText.Text = "PasswordBox changed (length " + passwordBoxDemo.Password.Length + ")";
        }

        private void TextBoxMultiline_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsReady) return;
            statusText.Text = "Multiline TextBox changed (" + textBoxMultiline.Text.Length + " chars)";
        }

        private void CheckBoxAgree_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsReady) return;
            statusText.Text = "CheckBox: " + (checkBoxAgree.IsChecked == true);
        }

        private void RadioOption_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsReady) return;
            RadioButton radioButton = (RadioButton)sender;
            statusText.Text = "RadioButton selected: " + radioButton.Content;
        }

        private void ComboBoxFive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsReady) return;
            if (comboBoxFive.SelectedItem is ComboBoxItem item)
            {
                statusText.Text = "ComboBox selected: " + item.Content;
            }
        }

        private void ComboBoxEditable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsReady) return;
            if (comboBoxEditable.SelectedItem is ComboBoxItem item)
            {
                statusText.Text = "Editable ComboBox selected: " + item.Content;
            }
        }

        private void ListBoxSingleSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsReady) return;
            if (listBoxSingleSelect.SelectedItem is ListBoxItem item)
            {
                statusText.Text = "ListBox (single) selected: " + item.Content;
            }
        }

        private void ListBoxMultiSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsReady) return;
            int count = listBoxMultiSelect.SelectedItems.Count;
            statusText.Text = "ListBox (multi) selected count: " + count;
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsReady) return;
            statusText.Text = "Slider (0-100) value: " + Math.Round(sliderVolume.Value);
        }

        private void SliderThreads_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsReady) return;
            statusText.Text = "Slider (1-16) value: " + Math.Round(sliderThreads.Value);
        }

        private void ToggleButtonDemo_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsReady) return;
            statusText.Text = "ToggleButton: " + (toggleButtonDemo.IsChecked == true);
        }

        private void TreeViewDemo_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!IsReady) return;
            if (treeViewDemo.SelectedItem is TreeViewItem item)
            {
                statusText.Text = "TreeView selected: " + item.Header;
            }
        }

        private void TabControlDemo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsReady) return;
            if (tabControlDemo.SelectedItem is TabItem tab)
            {
                statusText.Text = "Tab selected: " + tab.Header;
            }
        }

        private void DatePickerDemo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsReady) return;
            statusText.Text = "DatePicker date: " +
                (datePickerDemo.SelectedDate.HasValue ? datePickerDemo.SelectedDate.Value.ToShortDateString() : "(none)");
        }

        private void ButtonDemo_Click(object sender, RoutedEventArgs e)
        {
            if (!IsReady) return;
            statusText.Text = "Button clicked.";
        }
    }
}
