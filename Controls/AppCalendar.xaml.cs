using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace HabitTracker.Controls
{
    /// <summary>
    /// A small, self-contained month-view calendar used in place of the
    /// stock WPF DatePicker wherever we need per-day coloring (e.g. the
    /// Diary page marking logged / bookmarked days) and year navigation.
    /// </summary>
    public partial class AppCalendar : UserControl
    {
        public event EventHandler<DateTime>? DateSelected;

        public DateTime DisplayedMonth { get; private set; } =
            new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        private DateTime? _selectedDate;
        public DateTime? SelectedDate => _selectedDate;

        /// <summary>Dates rendered green (e.g. diary entries with content).</summary>
        public HashSet<DateTime> GreenDates { get; set; } = new();

        /// <summary>Dates rendered yellow (e.g. bookmarked dates). Takes visual
        /// priority over GreenDates - a bookmarked day is always yellow, whether
        /// or not it also has a diary entry.</summary>
        public HashSet<DateTime> YellowDates { get; set; } = new();

        public AppCalendar()
        {
            InitializeComponent();
            BuildWeekdayHeader();
            Loaded += (s, e) => Render();
        }

        /// <summary>
        /// Selects a date and jumps the visible month to it.
        /// Pass notify:true to raise DateSelected (used for external navigation
        /// sync); the calendar's own day-click always notifies.
        /// </summary>
        public void SetSelectedDate(DateTime date, bool notify = false)
        {
            _selectedDate = date.Date;
            DisplayedMonth = new DateTime(date.Year, date.Month, 1);
            Render();
            if (notify) DateSelected?.Invoke(this, date.Date);
        }

        public void Refresh() => Render();

        private void BuildWeekdayHeader()
        {
            WeekdayHeader.Children.Clear();
            string[] labels = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };
            foreach (var l in labels)
            {
                WeekdayHeader.Children.Add(new TextBlock
                {
                    Text = l,
                    Foreground = ResourceBrush("TextMutedBrush"),
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 4)
                });
            }
        }

        private Brush ResourceBrush(string key)
        {
            return (Brush)(TryFindResource(key) ?? Application.Current.Resources[key]);
        }

        private Style ResourceStyle(string key)
        {
            return (Style)(TryFindResource(key) ?? Application.Current.Resources[key]);
        }

        private void Render()
        {
            if (BtnMonthYear == null || DaysGrid == null) return; // not yet loaded

            BtnMonthYear.Content = DisplayedMonth.ToString("MMMM yyyy");

            DaysGrid.Children.Clear();
            DaysGrid.RowDefinitions.Clear();
            DaysGrid.ColumnDefinitions.Clear();

            for (int c = 0; c < 7; c++)
                DaysGrid.ColumnDefinitions.Add(new ColumnDefinition());

            DateTime firstOfMonth = DisplayedMonth;
            int startOffset = (int)firstOfMonth.DayOfWeek; // Sunday = 0
            int daysInMonth = DateTime.DaysInMonth(firstOfMonth.Year, firstOfMonth.Month);
            int totalCells = startOffset + daysInMonth;
            int rows = (int)Math.Ceiling(totalCells / 7.0);

            for (int r = 0; r < rows; r++)
                DaysGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(38) });

            var accentBrush = ResourceBrush("AccentBrush");
            var greenBrush = ResourceBrush("SuccessBrush");
            var yellowBrush = ResourceBrush("WarningBrush");
            var defaultBrush = ResourceBrush("CardHoverBrush");
            var textPrimary = ResourceBrush("TextPrimaryBrush");
            var dayStyle = ResourceStyle("CalendarDayButtonStyle");

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(firstOfMonth.Year, firstOfMonth.Month, day);
                int cellIndex = startOffset + day - 1;
                int row = cellIndex / 7;
                int col = cellIndex % 7;

                bool isSelected = _selectedDate.HasValue && _selectedDate.Value.Date == date;
                bool isToday = date == DateTime.Today;
                bool isGreen = GreenDates.Contains(date);
                bool isYellow = YellowDates.Contains(date);

                var btn = new Button
                {
                    Content = day.ToString(),
                    Tag = date,
                    Margin = new Thickness(2),
                    Style = dayStyle,
                    Cursor = Cursors.Hand,
                    BorderBrush = accentBrush,
                    BorderThickness = new Thickness(isToday && !isSelected ? 1.5 : 0),
                    FontWeight = isToday ? FontWeights.Bold : FontWeights.Normal
                };

                if (isSelected)
                {
                    btn.Background = accentBrush;
                    btn.Foreground = Brushes.White;
                }
                else if (isYellow)
                {
                    // Bookmarked days are always yellow, regardless of log status.
                    btn.Background = yellowBrush;
                    btn.Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                }
                else if (isGreen)
                {
                    btn.Background = greenBrush;
                    btn.Foreground = Brushes.White;
                }
                else
                {
                    btn.Background = defaultBrush;
                    btn.Foreground = textPrimary;
                }

                btn.Click += (s, e) =>
                {
                    if (s is Button b && b.Tag is DateTime d)
                        SetSelectedDate(d, notify: true);
                };

                Grid.SetRow(btn, row);
                Grid.SetColumn(btn, col);
                DaysGrid.Children.Add(btn);
            }
        }

        private void BtnPrevMonth_Click(object sender, RoutedEventArgs e)
        {
            DisplayedMonth = DisplayedMonth.AddMonths(-1);
            Render();
        }

        private void BtnNextMonth_Click(object sender, RoutedEventArgs e)
        {
            DisplayedMonth = DisplayedMonth.AddMonths(1);
            Render();
        }

        private void BtnPrevYear_Click(object sender, RoutedEventArgs e)
        {
            DisplayedMonth = DisplayedMonth.AddYears(-1);
            Render();
        }

        private void BtnNextYear_Click(object sender, RoutedEventArgs e)
        {
            DisplayedMonth = DisplayedMonth.AddYears(1);
            Render();
        }

        private void BtnMonthYear_Click(object sender, RoutedEventArgs e)
        {
            BuildYearList();
            YearPopup.IsOpen = true;
        }

        private void BuildYearList()
        {
            YearListPanel.Children.Clear();

            int centerYear = DisplayedMonth.Year;
            var style = ResourceStyle("YearItemButtonStyle");
            var accentBrush = ResourceBrush("AccentBrush");

            // Most recent years first, a reasonable window either side of the current view.
            for (int year = centerYear + 5; year >= centerYear - 12; year--)
            {
                int y = year;
                var btn = new Button
                {
                    Content = y.ToString(),
                    Margin = new Thickness(0, 1, 0, 1),
                    Style = style
                };

                if (y == DisplayedMonth.Year)
                {
                    btn.Background = accentBrush;
                    btn.Foreground = Brushes.White;
                }

                btn.Click += (s, e) =>
                {
                    DisplayedMonth = new DateTime(y, DisplayedMonth.Month, 1);
                    YearPopup.IsOpen = false;
                    Render();
                };

                YearListPanel.Children.Add(btn);
            }
        }
    }
}
