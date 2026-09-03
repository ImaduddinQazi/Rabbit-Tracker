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
            DpJump.SelectedDate = _currentDate;

            var entry = DataService.LoadDiaryEntry(_currentDate);
            TxtContent.Text = entry.Content;

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

        private void DpJump_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DpJump.SelectedDate.HasValue && DpJump.SelectedDate.Value.Date != _currentDate)
            {
                LoadDate(DpJump.SelectedDate.Value);
            }
        }

        // ========== Bookmarks ==========

        private void LoadBookmarks()
        {
            _bookmarks = DataService.LoadBookmarks()
                .OrderByDescending(b => b.Date)
                .ToList();

            BookmarksPanel.Children.Clear();

            if (_bookmarks.Count == 0)
            {
                BookmarksPanel.Children.Add(new TextBlock
                {
                    Text = "No bookmarks yet",
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
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
                    Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
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
                ? new SolidColorBrush(Color.FromRgb(0, 122, 204))
                : new SolidColorBrush(Color.FromRgb(63, 63, 70));
        }

        private void BtnBookmark_Click(object sender, RoutedEventArgs e)
        {
            var existing = _bookmarks.FirstOrDefault(b => b.Date.Date == _currentDate);

            if (existing != null)
            {
                // Already bookmarked → remove
                _bookmarks.Remove(existing);
            }
            else
            {
                // Add new bookmark
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