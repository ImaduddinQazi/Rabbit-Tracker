using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HabitTracker.Helpers;
using HabitTracker.Services;

namespace HabitTracker
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            var profile = DataService.LoadProfile();
            TxtGreeting.Text = $"Welcome back, {profile.Name}!";

            // ---- Today's Goals ----
            var goals = DataService.LoadGoals();
            var todayGoals = goals.Where(g => GoalHelper.ShouldShowOn(g, DateTime.Today)).ToList();
            int totalToday = todayGoals.Count;
            int completedToday = todayGoals.Count(g => g.CurrentProgress >= g.TargetCount);
            TxtTodayGoals.Text = $"{completedToday} / {totalToday}";

            // ---- Streaks & Year Activity ----
            // Goal completions are logged separately (activity.json) because
            // Goal.CurrentProgress itself resets every day and would lose history.
            var goalActivity = DataService.LoadGoalActivity();
            var diaryDates = DataService.GetDiaryDatesWithContent();

            var activeDates = new HashSet<DateTime>(
                goalActivity.Where(kv => kv.Value > 0)
                            .Select(kv => DateTime.Parse(kv.Key)));
            activeDates.UnionWith(diaryDates);

            int currentStreak = ComputeCurrentStreak(activeDates);
            int bestStreak = ComputeBestStreak(activeDates);
            int yearDays = activeDates.Count(d => d.Year == DateTime.Today.Year);

            TxtStreak.Text = $"{currentStreak} day{(currentStreak == 1 ? "" : "s")}";
            TxtBestStreak.Text = $"{bestStreak} day{(bestStreak == 1 ? "" : "s")}";
            TxtYearActivity.Text = $"{yearDays} day{(yearDays == 1 ? "" : "s")}";

            BuildHeatmap(goalActivity, diaryDates);
            LoadUpcomingReminders();
        }

        private int ComputeCurrentStreak(HashSet<DateTime> activeDates)
        {
            int streak = 0;
            DateTime cursor = DateTime.Today;

            // If nothing logged yet today, that shouldn't break an existing
            // streak - start checking from yesterday instead.
            if (!activeDates.Contains(cursor))
                cursor = cursor.AddDays(-1);

            while (activeDates.Contains(cursor))
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }
            return streak;
        }

        private int ComputeBestStreak(HashSet<DateTime> activeDates)
        {
            if (activeDates.Count == 0) return 0;

            int best = 0, current = 0;
            DateTime? prev = null;

            foreach (var d in activeDates.OrderBy(x => x))
            {
                current = (prev.HasValue && d == prev.Value.AddDays(1)) ? current + 1 : 1;
                best = Math.Max(best, current);
                prev = d;
            }
            return best;
        }

        private void BuildHeatmap(Dictionary<string, int> goalActivity, HashSet<DateTime> diaryDates)
        {
            HeatmapGrid.Children.Clear();
            HeatmapGrid.ColumnDefinitions.Clear();
            HeatmapGrid.RowDefinitions.Clear();

            for (int i = 0; i < 7; i++)
                HeatmapGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(14) });

            int totalWeeks = 53;
            for (int i = 0; i < totalWeeks; i++)
                HeatmapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });

            DateTime startDate = new DateTime(DateTime.Now.Year, 1, 1);
            while (startDate.DayOfWeek != DayOfWeek.Sunday)
                startDate = startDate.AddDays(-1);

            for (int week = 0; week < totalWeeks; week++)
            {
                for (int day = 0; day < 7; day++)
                {
                    DateTime current = startDate.AddDays(week * 7 + day);
                    if (current.Year != DateTime.Now.Year) continue;

                    string key = current.ToString("yyyy-MM-dd");
                    int points = goalActivity.TryGetValue(key, out int p) ? p : 0;
                    if (diaryDates.Contains(current)) points += 2;

                    int level = points switch
                    {
                        0 => 0,
                        1 => 1,
                        2 or 3 => 2,
                        4 or 5 => 3,
                        _ => 4
                    };

                    var rect = new Rectangle
                    {
                        Width = 11,
                        Height = 11,
                        RadiusX = 2,
                        RadiusY = 2,
                        Margin = new Thickness(1.5),
                        ToolTip = $"{current:dd MMM yyyy} \u2014 {points} pt{(points == 1 ? "" : "s")}"
                    };

                    rect.Fill = level switch
                    {
                        0 => (Brush)FindResource("HeatmapLevel0"),
                        1 => (Brush)FindResource("HeatmapLevel1"),
                        2 => (Brush)FindResource("HeatmapLevel2"),
                        3 => (Brush)FindResource("HeatmapLevel3"),
                        _ => (Brush)FindResource("HeatmapLevel4"),
                    };

                    Grid.SetRow(rect, day);
                    Grid.SetColumn(rect, week);
                    HeatmapGrid.Children.Add(rect);
                }
            }

            TxtYearLabel.Text = DateTime.Now.Year.ToString();
        }

        private void LoadUpcomingReminders()
        {
            var reminders = DataService.LoadReminders()
                .Where(r => !r.IsTriggered)
                .OrderBy(r => r.DateTime)
                .Take(4)
                .ToList();

            UpcomingRemindersPanel.Children.Clear();

            if (reminders.Count == 0)
            {
                UpcomingRemindersPanel.Children.Add(new TextBlock
                {
                    Text = "No upcoming reminders. Add one from the button above.",
                    Foreground = (Brush)FindResource("TextMutedBrush"),
                    FontSize = 13
                });
                return;
            }

            foreach (var r in reminders)
            {
                var row = new Border
                {
                    Background = (Brush)FindResource("CardHoverBrush"),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(14),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var left = new StackPanel();
                left.Children.Add(new TextBlock { Text = r.Title, Foreground = Brushes.White, FontWeight = FontWeights.SemiBold, FontSize = 14 });
                if (!string.IsNullOrWhiteSpace(r.Message))
                {
                    left.Children.Add(new TextBlock
                    {
                        Text = r.Message,
                        Foreground = (Brush)FindResource("TextSecondaryBrush"),
                        FontSize = 12,
                        Margin = new Thickness(0, 4, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    });
                }

                var when = new TextBlock
                {
                    Text = r.DateTime.ToString("dd MMM, HH:mm"),
                    Foreground = (Brush)FindResource("AccentBrush"),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetColumn(left, 0);
                Grid.SetColumn(when, 1);
                grid.Children.Add(left);
                grid.Children.Add(when);

                row.Child = grid;
                UpcomingRemindersPanel.Children.Add(row);
            }
        }

        private void BtnQuickAddGoal_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddGoalWindow { Owner = Window.GetWindow(this) };
            if (addWindow.ShowDialog() == true)
            {
                var goals = DataService.LoadGoals();
                goals.Add(addWindow.NewGoal);
                DataService.SaveGoals(goals);
            }
            (Application.Current.MainWindow as MainWindow)?.GoToDailyGoals();
        }

        private void BtnQuickDiary_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.GoToDiary();
        }

        private void BtnQuickAddReminder_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.AddReminderFlow();
            LoadUpcomingReminders();
        }
    }
}
