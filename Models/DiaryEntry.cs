using System;

namespace HabitTracker.Models
{
    public class DiaryEntry
    {
        public DateTime Date { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}