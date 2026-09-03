using System;

namespace HabitTracker.Models
{
    public class Reminder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public bool IsTriggered { get; set; } = false;
        public bool IsRead { get; set; } = false;
    }
}