using System;
using System.Windows;
using HabitTracker.Models;

namespace HabitTracker
{
    public partial class AddReminderWindow : Window
    {
        public Reminder NewReminder { get; private set; }

        public AddReminderWindow()
        {
            InitializeComponent();
            DpDate.SelectedDate = DateTime.Today;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                MessageBox.Show("Please enter a title.");
                return;
            }

            if (!int.TryParse(TxtHour.Text, out int hour) || hour < 0 || hour > 23 ||
                !int.TryParse(TxtMinute.Text, out int minute) || minute < 0 || minute > 59)
            {
                MessageBox.Show("Please enter a valid time (Hour 0-23, Minute 0-59).");
                return;
            }

            var date = DpDate.SelectedDate ?? DateTime.Today;
            var dateTime = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);

            if (dateTime <= DateTime.Now)
            {
                MessageBox.Show("Please choose a future date and time.");
                return;
            }

            NewReminder = new Reminder
            {
                Title = TxtTitle.Text.Trim(),
                Message = TxtMessage.Text.Trim(),
                DateTime = dateTime,
                IsTriggered = false,
                IsRead = false
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