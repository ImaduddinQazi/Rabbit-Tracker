using System;
using System.Collections.Generic;
using System.Windows;
using HabitTracker.Models;

namespace HabitTracker
{
    public partial class AddGoalWindow : Window
    {
        public Goal NewGoal { get; private set; }

        // Null when creating a new goal; set when editing an existing one.
        // Editing mutates this same object in place rather than creating a
        // new Goal, so the reference already in DailyGoalsPage's list stays
        // valid and its CurrentProgress/streak history isn't disturbed.
        private readonly Goal _editingGoal;

        private static readonly string[] RepeatOptions =
        {
            "Everyday",
            "Weekdays (Mon-Fri)",
            "Weekends (Sat-Sun)",
            "Only Saturday",
            "Only Sunday",
            "Custom days..."
        };

        public AddGoalWindow() : this(null) { }

        public AddGoalWindow(Goal goalToEdit)
        {
            InitializeComponent();
            _editingGoal = goalToEdit;

            CmbRepeat.SetItems(RepeatOptions, selectedIndex: 0);

            if (_editingGoal != null)
            {
                TxtHeader.Text = "Edit Goal";
                BtnSave.Content = "Save Changes";

                TxtTitle.Text = _editingGoal.Title;
                TxtCount.Text = _editingGoal.TargetCount.ToString();
                DpStart.SelectedDate = _editingGoal.StartDate;
                DpEnd.SelectedDate = _editingGoal.EndDate;

                int repeatIndex = _editingGoal.RepeatType switch
                {
                    "Everyday" => 0,
                    "Weekdays" => 1,
                    "Weekends" => 2,
                    "Saturday" => 3,
                    "Sunday" => 4,
                    "Custom" => 5,
                    _ => 0
                };
                CmbRepeat.SelectedIndex = repeatIndex;

                if (repeatIndex == 5)
                {
                    CustomDaysPanel.Visibility = Visibility.Visible;
                    CbMon.IsChecked = _editingGoal.CustomDays.Contains("Monday");
                    CbTue.IsChecked = _editingGoal.CustomDays.Contains("Tuesday");
                    CbWed.IsChecked = _editingGoal.CustomDays.Contains("Wednesday");
                    CbThu.IsChecked = _editingGoal.CustomDays.Contains("Thursday");
                    CbFri.IsChecked = _editingGoal.CustomDays.Contains("Friday");
                    CbSat.IsChecked = _editingGoal.CustomDays.Contains("Saturday");
                    CbSun.IsChecked = _editingGoal.CustomDays.Contains("Sunday");
                }
            }
            else
            {
                DpStart.SelectedDate = DateTime.Today;
                DpEnd.SelectedDate = DateTime.Today.AddMonths(3);
            }
        }

        private void CmbRepeat_SelectionChanged(object sender, int selectedIndex)
        {
            if (CustomDaysPanel == null) return;

            CustomDaysPanel.Visibility = selectedIndex == 5
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

            var startDate = DpStart.SelectedDate ?? DateTime.Today;
            var endDate = DpEnd.SelectedDate ?? DateTime.Today.AddMonths(3);

            if (_editingGoal != null)
            {
                // Mutate in place - CurrentProgress/LastProgressDate/IsActive are
                // left untouched so editing a goal doesn't reset today's progress.
                _editingGoal.Title = TxtTitle.Text.Trim();
                _editingGoal.TargetCount = count;
                _editingGoal.StartDate = startDate;
                _editingGoal.EndDate = endDate;
                _editingGoal.RepeatType = repeatType;
                _editingGoal.CustomDays = customDays;

                NewGoal = _editingGoal;
            }
            else
            {
                NewGoal = new Goal
                {
                    Title = TxtTitle.Text.Trim(),
                    TargetCount = count,
                    StartDate = startDate,
                    EndDate = endDate,
                    RepeatType = repeatType,
                    CustomDays = customDays,
                    CurrentProgress = 0,
                    LastProgressDate = DateTime.Today,
                    IsActive = true
                };
            }

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
