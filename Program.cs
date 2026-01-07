using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TrackFlow.Forms;
using TrackFlow.Service;

static class Program
{
    [STAThread]
    static void Main()
    {
        // ------------ DEBUG ---------------
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();
        AllocConsole(); // this is for debuging only; it allows the use of console even if the app is GUI so we can print stuff

        Task.Run(() => Tester.Start()); // starts test for services, it's a debug feature we will rmove this line!
        // ------------ DEBUG ---------------

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());

        // Verification test for my commits nothing relevent
    }
}
