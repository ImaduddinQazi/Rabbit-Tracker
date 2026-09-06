using System.Windows;
using System.Windows.Controls;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker
{
    public partial class FirstRunPage : Page
    {
        public FirstRunPage()
        {
            InitializeComponent();
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter your name.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var profile = new Profile
            {
                Name = name,
                Theme = "Dark", // only theme currently offered
                IsFirstRunCompleted = true
            };

            DataService.SaveProfile(profile);

            // Go to main window content
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.LoadMainApp();
        }
    }
}