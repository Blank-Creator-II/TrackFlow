using System;
using System.IO;
using TrackFlow.Models;
using TrackFlow.Utils;

namespace TrackFlow.Service;
public static class ReminderService // this is basically the easiest out of the bunch so if you read the other coments you will clearly understand this I am not writting comments
{
    public static (bool s, ID f) SaveReminder(Reminder r)
    {
        var reminder_data = new List<string>
        {
            $"[REMINDER]",
            $"Id={r.Id.PID}",
            $"SavedDate={r.SavedDate:O}",
            $"ReminderDate={r.ReminderDate:O}",
            $"State={r.State}",
            $"ReminderNote={r.ReminderNote ?? "<null>"}", // if the variable is null hard code <null> so it can be reconstacted later as null
            $"[END]"
        };

        string reminder_data_location = Path.Combine(FileHelper.BASE_DIR,"Data","Reminder",$"reminder_{Guid.NewGuid()}.txt");
        return (FileHelper.WriteFile(reminder_data_location,reminder_data),r.Id);
    }

    private static Reminder _LoadReminder(string[] raw_reminder_data)
    {
        bool start_reminder_fetch = false;

        Dictionary<string, string?> reminderData = new Dictionary<string, string?>();

        foreach (string line in raw_reminder_data)
        {
            if (line == "[REMINDER]")
            {
                start_reminder_fetch = true;
            }
            else if (line == "[END]")
            {
                break;
            }
            else if (start_reminder_fetch)
            {
                string[] parts = line.Split("=",2);
                if (parts.Length != 2) continue;

                reminderData[parts[0]] = parts[1] == "<null>" ? null : parts[1]; // if the value of the stored data is hard codded <null> it will recreate in to null data
            }
        }

        string[] id_parts = reminderData["Id"]!.Split("-",2); // the symbol "!" stops the compiler from spitting out warnings it means "I know better trust me" to the compiler
        var build_id = new ID
        {
            Type = id_parts[0],
            Value = Convert.ToInt32(id_parts[1])
        };

        var reminder = new Reminder
        {
            Id = build_id,
            SavedDate = Convert.ToDateTime(reminderData["SavedDate"]),
            ReminderDate = Convert.ToDateTime(reminderData["ReminderDate"]),
            State = Convert.ToBoolean(reminderData["State"]),
            ReminderNote = reminderData["ReminderNote"]
        };

        return reminder;
    }

    public static List<Reminder> LoadReminder()
    {
        List<Reminder> list_of_reminder = new List<Reminder>();
        List<string> location_list = FileHelper.FetchData("Reminder");

        foreach (string reminder_data_path in location_list)
        {
            list_of_reminder.Add(_LoadReminder(FileHelper.ReadFile(reminder_data_path)));
        }

        return list_of_reminder; 
    }

    public static bool DeleteReminder(ID id)
    {
        List<string> reminderFiles = FileHelper.FetchData("Reminder");

        foreach (string path in reminderFiles)
        {
            string[] rawData = FileHelper.ReadFile(path);

            foreach (string line in rawData)
            {
                if (line.StartsWith("Id="))
                {
                    if (line.Substring(3) == id.PID)
                    {
                        File.Delete(path);
                        return true;
                    }
                    break;
                }
            }
        }

        return false;
    }

    public static List<Reminder> SearchReminders(List<Reminder> reminders, object searchValue)
    {
        if (searchValue == null)
            return new List<Reminder>();

        string query = searchValue.ToString()!.ToLowerInvariant();
        var rankedResults = new List<(Reminder reminder, int score)>();

        foreach (Reminder r in reminders)
        {
            int score = 0;

            void Match(string? value, int exact, int partial)
            {
                if (string.IsNullOrEmpty(value)) return;

                string v = value.ToLowerInvariant();
                if (v == query) score += exact;
                else if (v.Contains(query)) score += partial;
            }

            // Core fields
            Match(r.Id.PID, 100, 50);
            Match(r.SavedDate.ToString("O"), 90, 45);
            Match(r.ReminderDate.ToString("O"), 95, 45);
            Match(r.State.ToString(), 80, 40);

            // Optional note
            Match(r.ReminderNote, 60, 30);

            if (score > 0)
                rankedResults.Add((r, score));
        }

        return rankedResults
            .OrderByDescending(r => r.score)
            .Select(r => r.reminder)
            .ToList();
    }
}