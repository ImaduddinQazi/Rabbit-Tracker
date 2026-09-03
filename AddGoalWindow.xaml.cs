using System;
using System.Collections.Generic;
using System.Windows;
using HabitTracker.Models;

namespace HabitTracker
{
    public partial class AddGoalWindow : Window
    {
        public Goal NewGoal { get; private set; }

        public AddGoalWindow()
        {
            InitializeComponent();
            DpStart.SelectedDate = DateTime.Today;
            DpEnd.SelectedDate = DateTime.Today.AddMonths(3);
        }

        private void CmbRepeat_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CustomDaysPanel == null) return;

            CustomDaysPanel.Visibility = CmbRepeat.SelectedIndex == 5
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                MessageBox.Show("Please enter a goal title.");
                return;
            }

            if (!int.TryParse(TxtCount.Text, out int count) || count < 1)
            {
                MessageBox.Show("Target count must be a number greater than 0.");
                return;
            }

            string repeatType = CmbRepeat.SelectedIndex switch
            {
                0 => "Everyday",
                1 => "Weekdays",
                2 => "Weekends",
                3 => "Saturday",
                4 => "Sunday",
                5 => "Custom",
                _ => "Everyday"
            };

            var customDays = new List<string>();
            if (repeatType == "Custom")
            {
                if (CbMon.IsChecked == true) customDays.Add("Monday");
                if (CbTue.IsChecked == true) customDays.Add("Tuesday");
                if (CbWed.IsChecked == true) customDays.Add("Wednesday");
                if (CbThu.IsChecked == true) customDays.Add("Thursday");
                if (CbFri.IsChecked == true) customDays.Add("Friday");
                if (CbSat.IsChecked == true) customDays.Add("Saturday");
                if (CbSun.IsChecked == true) customDays.Add("Sunday");

                if (customDays.Count == 0)
                {
                    MessageBox.Show("Please select at least one day for custom repeat.");
                    return;
                }
            }

            NewGoal = new Goal
            {
                Title = TxtTitle.Text.Trim(),
                TargetCount = count,
                StartDate = DpStart.SelectedDate ?? DateTime.Today,
                EndDate = DpEnd.SelectedDate ?? DateTime.Today.AddMonths(3),
                RepeatType = repeatType,
                CustomDays = customDays,
                CurrentProgress = 0,
                LastProgressDate = DateTime.Today,
                IsActive = true
            };

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}