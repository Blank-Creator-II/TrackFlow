using System;
using TrackFlow.Models;
using TrackFlow.Utils;

namespace TrackFlow.Service;
public static class ReminderService // this is basically the easiest out of the bunch so if you read the other coments you will clearly understand this I am not writting comments
{
    public static bool SaveReminder(Reminder r)
    {
        var reminder_data = new List<string>
        {
            $"[REMINDER]",
            $"Id={r.Id.PID}",
            $"SavedDate={r.SavedDate:O}",
            $"ReminderDate={r.ReminderDate:O}",
            $"State={r.State}",
            $"ReminderNote={r.ReminderNote}",
            $"[END]"
        };

        string reminder_data_location = Path.Combine(FileHelper.BASE_DIR,"Reminder",$"reminder_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        return FileHelper.WriteFile(reminder_data_location,reminder_data);
    }

    private static Reminder _LoadReminder(string[] raw_reminder_data)
    {
        bool start_reminder_fetch = false;

        Dictionary<string, string> reminderData = new Dictionary<string, string>();

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

                reminderData[parts[0]] = parts[1];
            }
        }

        string[] id_parts = reminderData["Id"].Split("-",2);
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
}