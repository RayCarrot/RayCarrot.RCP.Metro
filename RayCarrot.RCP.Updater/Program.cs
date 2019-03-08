using System;
using System.Windows.Forms;

namespace RayCarrot.RCP.Updater
{
    //
    //  Available launch arguments:
    //
    // -{filePath} {userLevel} {autoUpdate} (Automatically sets the path of the program to update) (Sets the current user level) (True if it should auto update)
    //
    internal static class Program
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            // Set default WinForms properties
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run the application
            Application.Run(new Form1(args));
        }
    }
}