using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _reminderTimer;
        private List<Reminder> _reminders = new();

        public MainWindow()
        {
            InitializeComponent();

            if (DataService.IsFirstRun())
            {
                MainFrame.Navigate(new FirstRunPage());
            }
            else
            {
                LoadMainApp();
            }

            StartReminderTimer();
        }

        public void LoadMainApp()
        {
            var profile = DataService.LoadProfile();
            TxtUserName.Text = $"Hello, {profile.Name}";

            MainFrame.Navigate(new DashboardPage());
            HighlightButton(BtnDashboard);
            LoadReminders();
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
            HighlightButton(BtnDashboard);
            NotificationPanel.Visibility = Visibility.Collapsed;
        }

        private void BtnDailyGoals_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DailyGoalsPage());
            HighlightButton(BtnDailyGoals);
            NotificationPanel.Visibility = Visibility.Collapsed;
        }

        private void BtnDiary_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DiaryPage());
            HighlightButton(BtnDiary);
            NotificationPanel.Visibility = Visibility.Collapsed;
        }

        private void HighlightButton(Button activeButton)
        {
            BtnDashboard.Background = Brushes.Transparent;
            BtnDailyGoals.Background = Brushes.Transparent;
            BtnDiary.Background = Brushes.Transparent;

            BtnDashboard.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));
            BtnDailyGoals.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));
            BtnDiary.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));

            activeButton.Background = new SolidColorBrush(Color.FromRgb(0, 122, 204));
            activeButton.Foreground = Brushes.White;
        }

        // ========== Reminders ==========

        private void LoadReminders()
        {
            _reminders = DataService.LoadReminders();
            RefreshNotificationList();
        }

        private void StartReminderTimer()
        {
            _reminderTimer = new DispatcherTimer();
            _reminderTimer.Interval = TimeSpan.FromSeconds(30); // check every 30 seconds
            _reminderTimer.Tick += ReminderTimer_Tick;
            _reminderTimer.Start();
        }

        private void ReminderTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            bool changed = false;

            foreach (var reminder in _reminders.Where(r => !r.IsTriggered))
            {
                if (reminder.DateTime <= now)
                {
                    reminder.IsTriggered = true;
                    changed = true;

                    // Show Windows notification
                    try
                    {
                        MessageBox.Show($"{reminder.Title}\n\n{reminder.Message}",
                                        "Reminder", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch { }
                }
            }

            if (changed)
            {
                DataService.SaveReminders(_reminders);
                RefreshNotificationList();
            }
        }

        private void BtnAddReminder_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddReminderWindow { Owner = this };
            if (win.ShowDialog() == true)
            {
                _reminders.Add(win.NewReminder);
                DataService.SaveReminders(_reminders);
                RefreshNotificationList();
            }
        }

        private void BtnNotifications_Click(object sender, RoutedEventArgs e)
        {
            NotificationPanel.Visibility = NotificationPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            RefreshNotificationList();
        }

        private void RefreshNotificationList()
        {
            NotificationsList.Children.Clear();

            var list = _reminders
                .OrderByDescending(r => r.DateTime)
                .Take(20)
                .ToList();

            if (list.Count == 0)
            {
                NotificationsList.Children.Add(new TextBlock
                {
                    Text = "No reminders yet",
                    Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
                    Margin = new Thickness(0, 20, 0, 0)
                });
                return;
            }

            foreach (var r in list)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var stack = new StackPanel();

                stack.Children.Add(new TextBlock
                {
                    Text = r.Title,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.White,
                    FontSize = 14
                });

                stack.Children.Add(new TextBlock
                {
                    Text = r.DateTime.ToString("dd MMM yyyy  HH:mm"),
                    Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)),
                    FontSize = 12,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                if (!string.IsNullOrWhiteSpace(r.Message))
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = r.Message,
                        Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 6, 0, 0)
                    });
                }

                if (r.IsTriggered)
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = "• Triggered",
                        Foreground = new SolidColorBrush(Color.FromRgb(78, 201, 176)),
                        FontSize = 11,
                        Margin = new Thickness(0, 6, 0, 0)
                    });
                }

                border.Child = stack;
                NotificationsList.Children.Add(border);
            }
        }
    }
}