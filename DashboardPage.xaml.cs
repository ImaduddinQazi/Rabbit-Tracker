using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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
            TxtGreeting.Text = $"Hello, {profile.Name}!";

            // Placeholder values for now (will be real later)
            TxtTodayGoals.Text = "0 / 0";
            TxtStreak.Text = "0 days";
            TxtYearActivity.Text = "0 days";

            BuildHeatmap();
        }

        private void BuildHeatmap()
        {
            HeatmapGrid.Children.Clear();
            HeatmapGrid.ColumnDefinitions.Clear();
            HeatmapGrid.RowDefinitions.Clear();

            // 7 rows (Sun → Sat), ~53 columns (weeks)
            for (int i = 0; i < 7; i++)
                HeatmapGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(14) });

            int totalWeeks = 53;
            for (int i = 0; i < totalWeeks; i++)
                HeatmapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });

            // Demo data – random activity levels (0-4)
            Random rand = new Random(DateTime.Now.DayOfYear); // consistent for the day

            DateTime startDate = new DateTime(DateTime.Now.Year, 1, 1);
            // Adjust to start from Sunday
            while (startDate.DayOfWeek != DayOfWeek.Sunday)
                startDate = startDate.AddDays(-1);

            for (int week = 0; week < totalWeeks; week++)
            {
                for (int day = 0; day < 7; day++)
                {
                    DateTime current = startDate.AddDays(week * 7 + day);

                    // Only show days of current year
                    if (current.Year != DateTime.Now.Year)
                        continue;

                    int level = rand.Next(0, 5); // 0 = none, 4 = highest

                    var rect = new Rectangle
                    {
                        Width = 11,
                        Height = 11,
                        RadiusX = 2,
                        RadiusY = 2,
                        Margin = new Thickness(1.5),
                        ToolTip = $"{current:dd MMM yyyy}"
                    };

                    rect.Fill = level switch
                    {
                        0 => new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                        1 => new SolidColorBrush(Color.FromRgb(14, 68, 41)),
                        2 => new SolidColorBrush(Color.FromRgb(0, 109, 50)),
                        3 => new SolidColorBrush(Color.FromRgb(38, 166, 65)),
                        4 => new SolidColorBrush(Color.FromRgb(57, 211, 83)),
                        _ => new SolidColorBrush(Color.FromRgb(45, 45, 48))
                    };

                    Grid.SetRow(rect, day);
                    Grid.SetColumn(rect, week);
                    HeatmapGrid.Children.Add(rect);
                }
            }

            TxtYearLabel.Text = DateTime.Now.Year.ToString();
        }
    }
}