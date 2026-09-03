using System;
using System.Collections.Generic;

namespace HabitTracker.Models
{
    public class Goal
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public int TargetCount { get; set; } = 1;
        public int CurrentProgress { get; set; } = 0;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(3);
        public bool IsActive { get; set; } = true;
        public DateTime LastProgressDate { get; set; } = DateTime.MinValue;

        // Repeating options
        // Possible values: "Everyday", "Weekdays", "Weekends", "Saturday", "Sunday", "Custom"
        public string RepeatType { get; set; } = "Everyday";

        // Only used when RepeatType == "Custom"
        // Example: ["Monday", "Wednesday", "Friday"]
        public List<string> CustomDays { get; set; } = new List<string>();
    }
}