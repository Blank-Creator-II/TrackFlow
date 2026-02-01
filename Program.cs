using TrackFlow.Forms;
using TrackFlow.Service;
using TrackFlow.Utils;
using System.Runtime.InteropServices;

static class Program
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)] // import the dll for stampind proocess ID, we need it for notifcation
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appID);

    [STAThread]
    static void Main(string[] args)
    {
        // we set the appId to this. Note this appId is set in start menu and stuff using Inno Setup
        // therefore a build that doesn't put this app's shortcut into stat menu might not have a notification support!
        SetCurrentProcessExplicitAppUserModelID("BLANK.TrackFlow");
        
        bool debug = args != null && Array.Exists(args, a => a == "--debug");
        bool testService = args != null && Array.Exists(args, a => a == "--test-service");
        FileHelper.CheckDataDirectory();

        if (debug)
        {
            Console.WriteLine("[DEBUG SESSION STARTED]");
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += (s, e) =>
            {
                Console.WriteLine("[UI THREAD EXCEPTION]");
                Console.WriteLine(e.Exception);
                Console.WriteLine("[OUTPUT ----- END]");
                Environment.Exit(1);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine("[NON-UI UNHANDLED EXCEPTION]");
                Console.WriteLine(e.ExceptionObject);
                Console.WriteLine("[OUTPUT ----- END]");
                Environment.Exit(1);
            };
        }

        try
        {
            if (debug && testService)
            {
                Tester.Start(args!);
            }
            else
            {
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
        }
        catch (Exception ex)
        {
            if (debug)
            {
                Console.WriteLine("[STARTUP FAILURE]");
                Console.WriteLine(ex);
                Console.WriteLine("[OUTPUT ----- END]");
                Environment.Exit(1);
            }
            throw;
        }
    }
}
