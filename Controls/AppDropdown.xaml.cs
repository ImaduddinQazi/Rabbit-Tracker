using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HabitTracker.Controls
{
    /// <summary>
    /// Replaces the stock WPF ComboBox, whose default dropdown popup rendered
    /// as a plain white box that clashed with the app's dark theme. This is a
    /// button that opens a themed popup list of plain-text options.
    /// </summary>
    public partial class AppDropdown : UserControl
    {
        public event EventHandler<int>? SelectionChanged;

        private List<string> _items = new();
        private int _selectedIndex = -1;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value < 0 || value >= _items.Count) return;
                _selectedIndex = value;
                UpdateTriggerText();
                HighlightSelected();
            }
        }

        public AppDropdown()
        {
            InitializeComponent();
        }

        /// <summary>Populates the dropdown. Does not raise SelectionChanged.</summary>
        public void SetItems(IEnumerable<string> items, int selectedIndex = 0)
        {
            _items = items.ToList();
            BuildItemButtons();
            SelectedIndex = selectedIndex;
        }

        private void UpdateTriggerText()
        {
            if (BtnTrigger != null && _selectedIndex >= 0 && _selectedIndex < _items.Count)
                BtnTrigger.Content = _items[_selectedIndex];
        }

        private void BuildItemButtons()
        {
            ItemsPanel.Children.Clear();

            for (int i = 0; i < _items.Count; i++)
            {
                int index = i;
                var btn = new Button
                {
                    Content = _items[i],
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 1, 0, 1),
                    Style = (Style)FindResource("DropdownItemButtonStyle")
                };
                btn.Click += (s, e) =>
                {
                    SelectedIndex = index;
                    DropdownPopup.IsOpen = false;
                    SelectionChanged?.Invoke(this, index);
                };
                ItemsPanel.Children.Add(btn);
            }
        }

        private void HighlightSelected()
        {
            for (int i = 0; i < ItemsPanel.Children.Count; i++)
            {
                if (ItemsPanel.Children[i] is Button b)
                {
                    bool isSel = i == _selectedIndex;
                    b.Background = isSel ? (Brush)FindResource("AccentBrush") : Brushes.Transparent;
                    b.Foreground = isSel ? Brushes.White : (Brush)FindResource("TextPrimaryBrush");
                }
            }
        }

        private void BtnTrigger_Click(object sender, RoutedEventArgs e)
        {
            DropdownPopup.IsOpen = true;
        }
    }
}
