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
    public partial class DiaryPage : Page
    {
        private DateTime _currentDate = DateTime.Today;
        private List<Bookmark> _bookmarks = new();
        private bool _isLoading = false;

        public DiaryPage()
        {
            InitializeComponent();
            LoadBookmarks();
            LoadDate(DateTime.Today);
        }

        private void LoadDate(DateTime date)
        {
            _isLoading = true;
            _currentDate = date.Date;

            TxtCurrentDate.Text = _currentDate.ToString("dddd, dd MMMM yyyy");

            var entry = DataService.LoadDiaryEntry(_currentDate);
            TxtContent.Text = entry.Content;

            Calendar.SetSelectedDate(_currentDate); // notify:false - avoids feedback loop with Calendar_DateSelected

            UpdateBookmarkButton();
            TxtStatus.Text = "Ready";
            _isLoading = false;
        }

        private void SaveCurrent()
        {
            if (_isLoading) return;

            var entry = new DiaryEntry
            {
                Date = _currentDate,
                Content = TxtContent.Text
            };
            DataService.SaveDiaryEntry(entry);
            TxtStatus.Text = $"Saved at {DateTime.Now:HH:mm:ss}";

            // Keep the calendar's "logged" (green) state in sync immediately,
            // without re-scanning every diary file on every keystroke.
            if (string.IsNullOrWhiteSpace(entry.Content))
                Calendar.GreenDates.Remove(_currentDate);
            else
                Calendar.GreenDates.Add(_currentDate);

            Calendar.Refresh();
        }

        private void TxtContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoading)
                SaveCurrent();
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            LoadDate(_currentDate.AddDays(-1));
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            LoadDate(_currentDate.AddDays(1));
        }

        private void BtnToday_Click(object sender, RoutedEventArgs e)
        {
            LoadDate(DateTime.Today);
        }

        private void Calendar_DateSelected(object sender, DateTime date)
        {
            if (date != _currentDate)
                LoadDate(date);
        }

        // ========== Search ==========

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = TxtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                SearchResultsPopup.IsOpen = false;
                return;
            }

            var results = DataService.SearchDiaryEntries(query);
            RenderSearchResults(results, query);
        }

        private void RenderSearchResults(List<(DateTime Date, string Snippet)> results, string query)
        {
            SearchResultsPanel.Children.Clear();

            if (results.Count == 0)
            {
                SearchResultsPanel.Children.Add(new TextBlock
                {
                    Text = $"No entries containing \"{query}\"",
                    Foreground = (Brush)FindResource("TextMutedBrush"),
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(8)
                });
                SearchResultsPopup.IsOpen = true;
                return;
            }

            foreach (var r in results.Take(25))
            {
                var stack = new StackPanel();
                stack.Children.Add(new TextBlock
                {
                    Text = r.Date.ToString("dd MMM yyyy"),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)FindResource("AccentBrush")
                });
                stack.Children.Add(new TextBlock
                {
                    Text = r.Snippet,
                    FontSize = 11,
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 0)
                });

                var btn = new Button
                {
                    Content = stack,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Style = (Style)FindResource("DropdownItemButtonStyle"),
                    Tag = r.Date,
                    Margin = new Thickness(0, 1, 0, 1)
                };
                btn.Click += (s, e) =>
                {
                    if (s is Button b && b.Tag is DateTime d)
                    {
                        SearchResultsPopup.IsOpen = false;
                        TxtSearch.Text = string.Empty;
                        LoadDate(d);
                    }
                };

                SearchResultsPanel.Children.Add(btn);
            }

            SearchResultsPopup.IsOpen = true;
        }

        // ========== Bookmarks ==========

        private void LoadBookmarks()
        {
            _bookmarks = DataService.LoadBookmarks()
                .OrderByDescending(b => b.Date)
                .ToList();

            Calendar.YellowDates = new HashSet<DateTime>(_bookmarks.Select(b => b.Date.Date));
            Calendar.GreenDates = DataService.GetDiaryDatesWithContent();
            Calendar.Refresh();

            BookmarksPanel.Children.Clear();

            if (_bookmarks.Count == 0)
            {
                BookmarksPanel.Children.Add(new TextBlock
                {
                    Text = "No bookmarks yet",
                    Foreground = (Brush)FindResource("TextMutedBrush"),
                    FontSize = 13,
                    Margin = new Thickness(0, 10, 0, 0)
                });
                return;
            }

            foreach (var bm in _bookmarks)
            {
                var btn = new Button
                {
                    Content = string.IsNullOrWhiteSpace(bm.Title)
                        ? bm.Date.ToString("dd MMM yyyy")
                        : bm.Title,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Height = 34,
                    Margin = new Thickness(0, 0, 0, 6),
                    Background = (Brush)FindResource("CardBrush"),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 0, 0, 0),
                    Tag = bm,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                btn.Click += (s, e) =>
                {
                    if (s is Button b && b.Tag is Bookmark bookmark)
                        LoadDate(bookmark.Date);
                };

                // Right-click context menu for Rename / Remove
                var menu = new ContextMenu();
                var renameItem = new MenuItem { Header = "Rename" };
                renameItem.Click += (s, e) => RenameBookmark(bm);
                var removeItem = new MenuItem { Header = "Remove" };
                removeItem.Click += (s, e) => RemoveBookmark(bm);

                menu.Items.Add(renameItem);
                menu.Items.Add(removeItem);
                btn.ContextMenu = menu;

                BookmarksPanel.Children.Add(btn);
            }
        }

        private void UpdateBookmarkButton()
        {
            bool isBookmarked = _bookmarks.Any(b => b.Date.Date == _currentDate);
            BtnBookmark.Content = isBookmarked ? "Bookmarked" : "Bookmark";
            BtnBookmark.Background = isBookmarked
                ? (Brush)FindResource("AccentBrush")
                : (Brush)FindResource("CardHoverBrush");
        }

        private void BtnBookmark_Click(object sender, RoutedEventArgs e)
        {
            var existing = _bookmarks.FirstOrDefault(b => b.Date.Date == _currentDate);

            if (existing != null)
            {
                _bookmarks.Remove(existing);
            }
            else
            {
                _bookmarks.Add(new Bookmark
                {
                    Date = _currentDate,
                    Title = _currentDate.ToString("dd MMM yyyy")
                });
            }

            DataService.SaveBookmarks(_bookmarks);
            LoadBookmarks();
            UpdateBookmarkButton();
        }

        private void RenameBookmark(Bookmark bm)
        {
            string newTitle = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter new name for this bookmark:",
                "Rename Bookmark",
                bm.Title);

            if (!string.IsNullOrWhiteSpace(newTitle))
            {
                bm.Title = newTitle.Trim();
                DataService.SaveBookmarks(_bookmarks);
                LoadBookmarks();
            }
        }

        private void RemoveBookmark(Bookmark bm)
        {
            _bookmarks.Remove(bm);
            DataService.SaveBookmarks(_bookmarks);
            LoadBookmarks();
            UpdateBookmarkButton();
        }
    }
}
