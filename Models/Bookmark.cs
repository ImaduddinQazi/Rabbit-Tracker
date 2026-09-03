using System;

namespace HabitTracker.Models
{
    public class Bookmark
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;   // user can rename
    }
}
