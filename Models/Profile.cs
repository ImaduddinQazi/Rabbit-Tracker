namespace HabitTracker.Models
{
    public class Profile
    {
        public string Name { get; set; } = string.Empty;
        public string Theme { get; set; } = "Dark"; // "Dark" or "Light"
        public bool IsFirstRunCompleted { get; set; } = false;
    }
}