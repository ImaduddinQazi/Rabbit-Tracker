using System;
using System.IO;
using System.Text.Json;
using HabitTracker.Models;
using System.Collections.Generic;
using System.Linq;

namespace HabitTracker.Services
{
    public static class DataService
    {
        private static readonly string AppFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HabitTracker");

        private static readonly string ProfilePath = Path.Combine(AppFolder, "profile.json");

        static DataService()
        {
            // Create folder if it doesn't exist
            if (!Directory.Exists(AppFolder))
                Directory.CreateDirectory(AppFolder);
        }

        public static Profile LoadProfile()
        {
            if (!File.Exists(ProfilePath))
                return new Profile();

            try
            {
                string json = File.ReadAllText(ProfilePath);
                return JsonSerializer.Deserialize<Profile>(json) ?? new Profile();
            }
            catch
            {
                return new Profile();
            }
        }

        public static void SaveProfile(Profile profile)
        {
            string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ProfilePath, json);
        }

        public static bool IsFirstRun()
        {
            var profile = LoadProfile();
            return !profile.IsFirstRunCompleted;
        }

        // ================= Goals =================

        private static readonly string GoalsPath = Path.Combine(AppFolder, "goals.json");

        public static List<Goal> LoadGoals()
        {
            if (!File.Exists(GoalsPath))
                return new List<Goal>();

            try
            {
                string json = File.ReadAllText(GoalsPath);
                return JsonSerializer.Deserialize<List<Goal>>(json) ?? new List<Goal>();
            }
            catch
            {
                return new List<Goal>();
            }
        }

        public static void SaveGoals(List<Goal> goals)
        {
            string json = JsonSerializer.Serialize(goals, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GoalsPath, json);
        }

        // ================= Diary =================

        private static readonly string DiaryFolder = Path.Combine(AppFolder, "Diary");
        private static readonly string BookmarksPath = Path.Combine(AppFolder, "bookmarks.json");

        public static DiaryEntry LoadDiaryEntry(DateTime date)
        {
            string yearFile = Path.Combine(DiaryFolder, $"diary_{date.Year}.json");

            if (!Directory.Exists(DiaryFolder))
                Directory.CreateDirectory(DiaryFolder);

            if (!File.Exists(yearFile))
                return new DiaryEntry { Date = date.Date };

            try
            {
                string json = File.ReadAllText(yearFile);
                var list = JsonSerializer.Deserialize<List<DiaryEntry>>(json) ?? new List<DiaryEntry>();
                var entry = list.FirstOrDefault(e => e.Date.Date == date.Date);
                return entry ?? new DiaryEntry { Date = date.Date };
            }
            catch
            {
                return new DiaryEntry { Date = date.Date };
            }
        }

        public static void SaveDiaryEntry(DiaryEntry entry)
        {
            if (!Directory.Exists(DiaryFolder))
                Directory.CreateDirectory(DiaryFolder);

            string yearFile = Path.Combine(DiaryFolder, $"diary_{entry.Date.Year}.json");
            List<DiaryEntry> list = new();

            if (File.Exists(yearFile))
            {
                try
                {
                    string json = File.ReadAllText(yearFile);
                    list = JsonSerializer.Deserialize<List<DiaryEntry>>(json) ?? new List<DiaryEntry>();
                }
                catch { }
            }

            var existing = list.FirstOrDefault(e => e.Date.Date == entry.Date.Date);
            if (existing != null)
            {
                existing.Content = entry.Content;
                existing.LastModified = DateTime.Now;
            }
            else
            {
                entry.LastModified = DateTime.Now;
                list.Add(entry);
            }

            string newJson = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(yearFile, newJson);
        }

        /// <summary>
        /// Scans every stored diary year-file and returns the set of dates
        /// that have non-empty content. Used to color the Diary calendar
        /// green and to feed the Dashboard's activity heatmap/streak.
        /// </summary>
        public static HashSet<DateTime> GetDiaryDatesWithContent()
        {
            var result = new HashSet<DateTime>();
            if (!Directory.Exists(DiaryFolder))
                return result;

            foreach (var file in Directory.GetFiles(DiaryFolder, "diary_*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var list = JsonSerializer.Deserialize<List<DiaryEntry>>(json) ?? new List<DiaryEntry>();
                    foreach (var e in list)
                    {
                        if (!string.IsNullOrWhiteSpace(e.Content))
                            result.Add(e.Date.Date);
                    }
                }
                catch { }
            }

            return result;
        }

        /// <summary>
        /// Searches every stored diary entry for a keyword (case-insensitive)
        /// and returns matches as (date, short surrounding snippet), most
        /// recent first.
        /// </summary>
        public static List<(DateTime Date, string Snippet)> SearchDiaryEntries(string keyword)
        {
            var results = new List<(DateTime, string)>();
            if (string.IsNullOrWhiteSpace(keyword) || !Directory.Exists(DiaryFolder))
                return results;

            foreach (var file in Directory.GetFiles(DiaryFolder, "diary_*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var list = JsonSerializer.Deserialize<List<DiaryEntry>>(json) ?? new List<DiaryEntry>();

                    foreach (var e in list)
                    {
                        if (string.IsNullOrWhiteSpace(e.Content)) continue;

                        int idx = e.Content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                        if (idx < 0) continue;

                        int start = Math.Max(0, idx - 30);
                        int len = Math.Min(e.Content.Length - start, keyword.Length + 60);
                        string snippet = e.Content.Substring(start, len).Trim();

                        if (start > 0) snippet = "\u2026" + snippet;
                        if (start + len < e.Content.Length) snippet += "\u2026";

                        results.Add((e.Date.Date, snippet));
                    }
                }
                catch { }
            }

            return results.OrderByDescending(r => r.Item1).ToList();
        }

        // ================= Bookmarks =================

        public static List<Bookmark> LoadBookmarks()
        {
            if (!File.Exists(BookmarksPath))
                return new List<Bookmark>();

            try
            {
                string json = File.ReadAllText(BookmarksPath);
                return JsonSerializer.Deserialize<List<Bookmark>>(json) ?? new List<Bookmark>();
            }
            catch
            {
                return new List<Bookmark>();
            }
        }

        public static void SaveBookmarks(List<Bookmark> bookmarks)
        {
            string json = JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(BookmarksPath, json);
        }

        // ================= Reminders =================

        private static readonly string RemindersPath = Path.Combine(AppFolder, "reminders.json");

        public static List<Reminder> LoadReminders()
        {
            if (!File.Exists(RemindersPath))
                return new List<Reminder>();

            try
            {
                string json = File.ReadAllText(RemindersPath);
                return JsonSerializer.Deserialize<List<Reminder>>(json) ?? new List<Reminder>();
            }
            catch
            {
                return new List<Reminder>();
            }
        }

        public static void SaveReminders(List<Reminder> reminders)
        {
            string json = JsonSerializer.Serialize(reminders, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(RemindersPath, json);
        }

        // ================= Activity (goal-completion log for Dashboard) =================

        private static readonly string ActivityPath = Path.Combine(AppFolder, "activity.json");

        /// <summary>
        /// date (yyyy-MM-dd) -> number of goal-completion actions logged that day.
        /// This is what makes the Dashboard's streaks and heatmap real instead
        /// of random placeholder data - goal progress itself resets every day,
        /// so completions need to be logged separately to keep history.
        /// </summary>
        public static Dictionary<string, int> LoadGoalActivity()
        {
            if (!File.Exists(ActivityPath))
                return new Dictionary<string, int>();

            try
            {
                string json = File.ReadAllText(ActivityPath);
                return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }

        public static void SaveGoalActivity(Dictionary<string, int> activity)
        {
            string json = JsonSerializer.Serialize(activity, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ActivityPath, json);
        }

        public static void AddGoalCompletionPoint(DateTime date)
        {
            var activity = LoadGoalActivity();
            string key = date.ToString("yyyy-MM-dd");
            activity[key] = activity.TryGetValue(key, out int v) ? v + 1 : 1;
            SaveGoalActivity(activity);
        }

        public static void RemoveGoalCompletionPoint(DateTime date)
        {
            var activity = LoadGoalActivity();
            string key = date.ToString("yyyy-MM-dd");
            if (activity.TryGetValue(key, out int v) && v > 0)
            {
                activity[key] = v - 1;
                SaveGoalActivity(activity);
            }
        }
    }
}
