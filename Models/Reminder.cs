namespace TrackFlow.Models;
public class Reminder
{
    public required ID Id {get; set;} // Id used for linking and sarching function
    public required DateTime SavedDate {get; set;} // the saved date of the reminder
    public required DateTime ReminderDate {get; set;} // the data that the reminder should... well remind you duh
    public bool State {get; set;} = true; // this is the state of the reminder after setting the reminder the user might not want it just like an alarm
    public string? ReminderNote {get; set;} // optional value to store a simpe string with the reminder like stiky notes
    public ID? Link {get; set;} // used for todo linking
}