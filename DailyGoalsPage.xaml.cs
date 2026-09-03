using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker
{
    public partial class DailyGoalsPage : Page
    {
        private List<Goal> _allGoals = new();

        public DailyGoalsPage()
        {
            InitializeComponent();
            TxtToday.Text = $"Today • {DateTime.Today:dddd, dd MMMM yyyy}";
            LoadGoals();
        }

        private void LoadGoals()
        {
            _allGoals = DataService.LoadGoals();

            // Reset daily progress if new day
            bool changed = false;
            foreach (var goal in _allGoals)
            {
                if (goal.LastProgressDate.Date != DateTime.Today)
                {
                    goal.CurrentProgress = 0;
                    goal.LastProgressDate = DateTime.Today;
                    changed = true;
                }
            }
            if (changed)
                DataService.SaveGoals(_allGoals);

            RefreshList();
        }

        private bool ShouldShowToday(Goal goal)
        {
            if (!goal.IsActive) return false;
            if (DateTime.Today < goal.StartDate.Date || DateTime.Today > goal.EndDate.Date)
                return false;

            string today = DateTime.Today.DayOfWeek.ToString(); // "Monday", "Sunday"...

            return goal.RepeatType switch
            {
                "Everyday" => true,
                "Weekdays" => today is "Monday" or "Tuesday" or "Wednesday" or "Thursday" or "Friday",
                "Weekends" => today is "Saturday" or "Sunday",
                "Saturday" => today == "Saturday",
                "Sunday" => today == "Sunday",
                "Custom" => goal.CustomDays.Contains(today),
                _ => true
            };
        }

        private void RefreshList()
        {
            GoalsPanel.Children.Clear();

            var todayGoals = _allGoals
                .Where(g => ShouldShowToday(g) && g.CurrentProgress < g.TargetCount)
                .OrderBy(g => g.Title)
                .ToList();

            if (todayGoals.Count == 0)
            {
                var empty = new TextBlock
                {
                    Text = "No goals scheduled for today.\nClick \"+ Add Goal\" to create one.",
                    Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
                    FontSize = 15,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 70, 0, 0)
                };
                GoalsPanel.Children.Add(empty);
                return;
            }

            foreach (var goal in todayGoals)
            {
                GoalsPanel.Children.Add(CreateGoalCard(goal));
            }
        }

        private Border CreateGoalCard(Goal goal)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Left side - Title + Progress
            var left = new StackPanel();

            var title = new TextBlock
            {
                Text = goal.Title,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };

            var progressText = new TextBlock
            {
                Text = goal.TargetCount == 1
                    ? "Click to complete"
                    : $"Progress: {goal.CurrentProgress} / {goal.TargetCount}",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
                Margin = new Thickness(0, 6, 0, 0)
            };

            left.Children.Add(title);
            left.Children.Add(progressText);

            // Right side - Complete button
            var btn = new Button
            {
                Content = goal.TargetCount == 1 ? "Complete" : "+1",
                Width = 90,
                Height = 34,
                Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = goal
            };
            btn.Click += BtnComplete_Click;

            Grid.SetColumn(left, 0);
            Grid.SetColumn(btn, 1);

            grid.Children.Add(left);
            grid.Children.Add(btn);
            card.Child = grid;

            return card;
        }

        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Goal goal)
            {
                goal.CurrentProgress++;
                goal.LastProgressDate = DateTime.Today;

                DataService.SaveGoals(_allGoals);
                RefreshList();
            }
        }

        private void BtnAddGoal_Click(object sender, RoutedEventArgs e)
        {
            // We will create a proper Add Goal window next
            var addWindow = new AddGoalWindow();
            addWindow.Owner = Window.GetWindow(this);

            if (addWindow.ShowDialog() == true)
            {
                _allGoals.Add(addWindow.NewGoal);
                DataService.SaveGoals(_allGoals);
                RefreshList();
            }
        }
    }
}