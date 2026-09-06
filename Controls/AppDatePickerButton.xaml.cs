using System;
using System.Windows;
using System.Windows.Controls;

namespace HabitTracker.Controls
{
    /// <summary>
    /// Replaces the stock WPF DatePicker, whose default popup rendered with a
    /// near-white background and low-contrast text on this app's dark theme.
    /// This is just a button showing the chosen date, which opens our own
    /// AppCalendar in a popup - guaranteed to match the rest of the app.
    /// </summary>
    public partial class AppDatePickerButton : UserControl
    {
        private DateTime? _selectedDate;

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                UpdateButtonText();
            }
        }

        public AppDatePickerButton()
        {
            InitializeComponent();
            UpdateButtonText();
        }

        private void UpdateButtonText()
        {
            if (BtnTrigger == null) return;
            BtnTrigger.Content = _selectedDate.HasValue
                ? _selectedDate.Value.ToString("dd MMM yyyy")
                : "Select date";
        }

        private void BtnTrigger_Click(object sender, RoutedEventArgs e)
        {
            InnerCalendar.SetSelectedDate(_selectedDate ?? DateTime.Today);
            CalendarPopup.IsOpen = true;
        }

        private void InnerCalendar_DateSelected(object sender, DateTime date)
        {
            SelectedDate = date;
            CalendarPopup.IsOpen = false;
        }
    }
}
