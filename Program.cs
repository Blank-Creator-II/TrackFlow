using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackFlow.Forms;
using TrackFlow.Service;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        bool debug = args != null && Array.Exists(args, a => a == "--debug");
        bool testService = args != null && Array.Exists(args, a => a == "--test-service");

        if (debug)
        {
            Console.WriteLine("[DEBUG SESSION STARTED]");
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += (s, e) =>
            {
                Console.WriteLine("[UI THREAD EXCEPTION]");
                Console.WriteLine(e.Exception);
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
                Environment.Exit(1);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine("[NON-UI UNHANDLED EXCEPTION]");
                Console.WriteLine(e.ExceptionObject);
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
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
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
                Environment.Exit(1);
            }
            throw;
        }
    }
}
