using System.Runtime.InteropServices;
using Microsoft.Toolkit.Uwp.Notifications;

public static class Notification
{
    public static void Show(string message)
    {
        string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notify.ico");
        Uri iconUri = new Uri("file:///" + iconPath);

        new ToastContentBuilder()
            .AddHeader("BLANK.TrackFlow.Reminder","TrackFlow Reminder","")
            .AddAppLogoOverride(iconUri)
            .AddText(message)
            .SetToastScenario(ToastScenario.Reminder)
            .SetToastDuration(ToastDuration.Long)
            .AddAudio(new ToastAudio{ Src = new Uri("ms-winsoundevent:Notification.Reminder"), Loop = true})
            .AddButton(new ToastButton()
                .SetContent("Stop")
                .AddArgument("action", "stop"))
            .Show();
    }
}

internal static class Program
{   
    private const string HelperID = "BLANK.TrackFlow.Helper";

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appID);
    static AutoResetEvent _waitHandle = new AutoResetEvent(false);

    [STAThread]
    static void Main(string[] args)
    {
        SetCurrentProcessExplicitAppUserModelID(HelperID);

        // Check if the app was opened by the "Stop" button
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            // Parse the arguments returned from the click
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

            // Retrieve specific values added via .AddArgument()
            if (args.TryGetValue("action", out string actionValue))
            {
                if (actionValue == "stop")
                {
                    // Clear all notifications to silence the looping sound
                    ToastNotificationManagerCompat.History.Clear();
                }
            }
            
            // Exit the application properly regardless of the action
            _waitHandle.Set();
        };

        // Check if we were not launched by a toast click
        if (!ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
        {
            string message = args.Length > 0 ? string.Join(" ", args) : "A Reminder Was Set For Now";
            Notification.Show(message);
        }

	    _waitHandle.WaitOne(TimeSpan.FromHours(1)); // Wait for interaction, but auto-kill after 1 hour just in case
    }
}
