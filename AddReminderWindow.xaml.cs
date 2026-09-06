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

            // Default the time to a few minutes from now (rather than a fixed
            // "09:00") so the dialog validates successfully out of the box -
            // a hardcoded default is almost always in the past by the time
            // someone opens this dialog, which made Save look broken.
            // Uses suggested.Date (not DateTime.Today) so this still works
            // correctly if "+5 minutes" happens to cross midnight.
            var suggested = DateTime.Now.AddMinutes(5);
            DpDate.SelectedDate = suggested.Date;
            TxtHour.Text = suggested.ToString("HH");
            TxtMinute.Text = suggested.ToString("mm");
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