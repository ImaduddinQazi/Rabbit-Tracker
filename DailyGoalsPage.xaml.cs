using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HabitTracker.Helpers;
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
            TxtToday.Text = $"Today \u2022 {DateTime.Today:dddd, dd MMMM yyyy}";
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

        private void RefreshList()
        {
            GoalsPanel.Children.Clear();

            // Completed goals are kept in the list (not hidden) - they're just
            // sorted to the bottom and rendered with a strikethrough.
            var todayGoals = _allGoals
                .Where(g => GoalHelper.ShouldShowOn(g, DateTime.Today))
                .OrderBy(g => g.CurrentProgress >= g.TargetCount)
                .ThenBy(g => g.Title)
                .ToList();

            if (todayGoals.Count == 0)
            {
                var empty = new TextBlock
                {
                    Text = "No goals scheduled for today.\nClick \"+ Add Goal\" to create one.",
                    Foreground = (Brush)FindResource("TextMutedBrush"),
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
            bool isComplete = goal.CurrentProgress >= goal.TargetCount;

            var card = new Border
            {
                Style = (Style)FindResource("GoalCardStyle"),
                Opacity = isComplete ? 0.55 : 1.0
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
                Foreground = Brushes.White,
                TextDecorations = isComplete ? TextDecorations.Strikethrough : null
            };

            var progressText = new TextBlock
            {
                Text = isComplete
                    ? "Completed \u2713"
                    : (goal.TargetCount == 1 ? "Click to complete" : $"Progress: {goal.CurrentProgress} / {goal.TargetCount}"),
                FontSize = 13,
                Foreground = isComplete ? (Brush)FindResource("SuccessBrush") : (Brush)FindResource("TextSecondaryBrush"),
                Margin = new Thickness(0, 6, 0, 0)
            };

            left.Children.Add(title);
            left.Children.Add(progressText);

            Grid.SetColumn(left, 0);
            grid.Children.Add(left);

            if (!isComplete)
            {
                // Right side - Complete button (only shown while incomplete)
                var btn = new Button
                {
                    Content = goal.TargetCount == 1 ? "Complete" : "+1",
                    Width = 90,
                    Height = 36,
                    Style = (Style)FindResource("PrimaryButtonStyle"),
                    Tag = goal
                };
                btn.Click += BtnComplete_Click;

                Grid.SetColumn(btn, 1);
                grid.Children.Add(btn);
            }
            else
            {
                var check = new TextBlock
                {
                    Text = "\u2713",
                    FontSize = 22,
                    FontWeight = FontWeights.Bold,
                    Foreground = (Brush)FindResource("SuccessBrush"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                Grid.SetColumn(check, 1);
                grid.Children.Add(check);
            }

            card.Child = grid;
            return card;
        }

        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Goal goal)
            {
                if (goal.CurrentProgress >= goal.TargetCount) return; // safety guard

                goal.CurrentProgress++;
                goal.LastProgressDate = DateTime.Today;

                DataService.SaveGoals(_allGoals);
                DataService.AddGoalCompletionPoint(DateTime.Today); // feeds Dashboard streak/heatmap

                RefreshList();
            }
        }

        private void BtnAddGoal_Click(object sender, RoutedEventArgs e)
        {
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
