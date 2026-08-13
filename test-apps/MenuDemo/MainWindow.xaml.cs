using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MenuDemo
{
    public partial class MainWindow : Window
    {
        private static readonly Dictionary<string, string> BreadcrumbNames = new Dictionary<string, string>
        {
            { "main", "main menu" },
            { "start", "start" },
            { "options", "options" },
            { "settings", "settings" },
            { "audio", "audio" },
            { "video", "video" },
        };

        private readonly Dictionary<string, StackPanel> panels;
        private readonly List<string> navigationStack = new List<string> { "main" };

        public MainWindow()
        {
            InitializeComponent();

            panels = new Dictionary<string, StackPanel>
            {
                { "main", panel_main },
                { "start", panel_start },
                { "options", panel_options },
                { "settings", panel_settings },
                { "audio", panel_audio },
                { "video", panel_video },
            };

            ShowScreen("main");
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e) => NavigateTo("start");
        private void BtnOptions_Click(object sender, RoutedEventArgs e) => NavigateTo("options");
        private void BtnSettings_Click(object sender, RoutedEventArgs e) => NavigateTo("settings");
        private void BtnVideoShortcut_Click(object sender, RoutedEventArgs e) => NavigateTo("video");
        private void BtnAudio_Click(object sender, RoutedEventArgs e) => NavigateTo("audio");
        private void BtnVideo_Click(object sender, RoutedEventArgs e) => NavigateTo("video");

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (navigationStack.Count > 1)
            {
                navigationStack.RemoveAt(navigationStack.Count - 1);
                ShowScreen(navigationStack[navigationStack.Count - 1]);
            }
        }

        private void LeafButton_Click(object sender, RoutedEventArgs e)
        {
            string name = (string)((Button)sender).Tag;
            statusText.Text = "selected: " + name;
        }

        private void NavigateTo(string key)
        {
            navigationStack.Add(key);
            ShowScreen(key);
        }

        private void ShowScreen(string key)
        {
            foreach (KeyValuePair<string, StackPanel> entry in panels)
            {
                entry.Value.Visibility = entry.Key == key ? Visibility.Visible : Visibility.Collapsed;
            }

            breadcrumb.Text = string.Join(" / ", navigationStack.Select(k => BreadcrumbNames[k]));
        }
    }
}
