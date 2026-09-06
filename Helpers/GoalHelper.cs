using System;
using System.Linq;
using HabitTracker.Models;

namespace HabitTracker.Helpers
{
    public static class GoalHelper
    {
        /// <summary>
        /// Shared scheduling logic: is this goal due on the given date?
        /// Used by both DailyGoalsPage and DashboardPage so the two pages
        /// never disagree about what "today's goals" means.
        /// </summary>
        public static bool ShouldShowOn(Goal goal, DateTime date)
        {
            if (!goal.IsActive) return false;
            if (date.Date < goal.StartDate.Date || date.Date > goal.EndDate.Date)
                return false;

            string dayName = date.DayOfWeek.ToString();

            return goal.RepeatType switch
            {
                "Everyday" => true,
                "Weekdays" => dayName is "Monday" or "Tuesday" or "Wednesday" or "Thursday" or "Friday",
                "Weekends" => dayName is "Saturday" or "Sunday",
                "Saturday" => dayName == "Saturday",
                "Sunday" => dayName == "Sunday",
                "Custom" => goal.CustomDays.Contains(dayName),
                _ => true
            };
        }
    }
}
