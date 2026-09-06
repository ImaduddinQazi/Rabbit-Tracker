using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
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

        // Public wrappers so other pages (e.g. Dashboard quick actions) can navigate
        // without needing internal knowledge of the button click handlers.
        public void GoToDashboard() => BtnDashboard_Click(this, new RoutedEventArgs());
        public void GoToDailyGoals() => BtnDailyGoals_Click(this, new RoutedEventArgs());
        public void GoToDiary() => BtnDiary_Click(this, new RoutedEventArgs());

        private void HighlightButton(Button activeButton)
        {
            var inactiveBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204));

            BtnDashboard.Background = Brushes.Transparent;
            BtnDailyGoals.Background = Brushes.Transparent;
            BtnDiary.Background = Brushes.Transparent;

            BtnDashboard.Foreground = inactiveBrush;
            BtnDailyGoals.Foreground = inactiveBrush;
            BtnDiary.Foreground = inactiveBrush;

            activeButton.Background = (Brush)FindResource("AccentBrush");
            activeButton.Foreground = Brushes.White;
        }

        // ========== Reminders ==========

        private void LoadReminders()
        {
            _reminders = DataService.LoadReminders();
            RefreshNotificationList();
            UpdateBadge();

            // Catch anything that was already due while the app was closed,
            // instead of waiting up to 30 seconds for the first timer tick.
            ReminderTimer_Tick(this, EventArgs.Empty);
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
                    ShowToast(reminder);
                }
            }

            if (changed)
            {
                DataService.SaveReminders(_reminders);
                RefreshNotificationList();
                UpdateBadge();
            }
        }

        public void AddReminderFlow()
        {
            var win = new AddReminderWindow { Owner = this };
            if (win.ShowDialog() == true)
            {
                _reminders.Add(win.NewReminder);
                DataService.SaveReminders(_reminders);
                RefreshNotificationList();
                UpdateBadge();
            }
        }

        private void BtnAddReminder_Click(object sender, RoutedEventArgs e) => AddReminderFlow();

        private void BtnNotifications_Click(object sender, RoutedEventArgs e)
        {
            bool willShow = NotificationPanel.Visibility != Visibility.Visible;
            NotificationPanel.Visibility = willShow ? Visibility.Visible : Visibility.Collapsed;

            if (willShow)
            {
                // Opening the panel marks triggered reminders as read and clears the badge.
                bool changed = false;
                foreach (var r in _reminders.Where(rem => rem.IsTriggered && !rem.IsRead))
                {
                    r.IsRead = true;
                    changed = true;
                }
                if (changed)
                    DataService.SaveReminders(_reminders);

                UpdateBadge();
            }

            RefreshNotificationList();
        }

        private void UpdateBadge()
        {
            int unread = _reminders.Count(r => r.IsTriggered && !r.IsRead);

            if (unread > 0)
            {
                NotificationBadge.Visibility = Visibility.Visible;
                TxtBadgeCount.Text = unread > 9 ? "9+" : unread.ToString();
            }
            else
            {
                NotificationBadge.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// In-app toast card shown when a reminder fires. Replaces the old
        /// MessageBox.Show popup, which could appear behind the main window
        /// (or another app) and make it look like nothing happened.
        /// </summary>
        private void ShowToast(Reminder reminder)
        {
            var card = new Border
            {
                Background = (Brush)FindResource("CardBrush"),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 10),
                BorderBrush = (Brush)FindResource("AccentBrush"),
                BorderThickness = new Thickness(1),
                Effect = new DropShadowEffect { BlurRadius = 16, ShadowDepth = 2, Opacity = 0.4, Color = Colors.Black }
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = "\u23F0 " + reminder.Title,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            });

            if (!string.IsNullOrWhiteSpace(reminder.Message))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = reminder.Message,
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0)
                });
            }

            card.Child = stack;
            ToastContainer.Children.Insert(0, card);

            var dismissTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(7) };
            dismissTimer.Tick += (s, e) =>
            {
                dismissTimer.Stop();
                ToastContainer.Children.Remove(card);
            };
            dismissTimer.Start();
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
                    Foreground = (Brush)FindResource("TextMutedBrush"),
                    Margin = new Thickness(0, 20, 0, 0)
                });
                return;
            }

            foreach (var r in list)
            {
                bool unread = r.IsTriggered && !r.IsRead;

                var border = new Border
                {
                    Background = (Brush)FindResource("CardBrush"),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8),
                    BorderBrush = unread ? (Brush)FindResource("AccentBrush") : Brushes.Transparent,
                    BorderThickness = new Thickness(0, 0, 0, unread ? 3 : 0)
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
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontSize = 12,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                if (!string.IsNullOrWhiteSpace(r.Message))
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = r.Message,
                        Foreground = (Brush)FindResource("TextSecondaryBrush"),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 6, 0, 0)
                    });
                }

                if (r.IsTriggered)
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = "\u2022 Triggered",
                        Foreground = (Brush)FindResource("SuccessBrush"),
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
