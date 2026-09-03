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
    }
}