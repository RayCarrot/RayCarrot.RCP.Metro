using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The custom entry point for the application
    /// </summary>
    public static class Entry
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // We want to be careful what we do here since no error handling has been initialized, thus it can cause a silent crash.
            // For this we check for a specific launch argument used to indicate that we're running in no-ui mode.
            if (args.Length > 0 && args[0] == "-noui")
            {
                RunWithNoUI(args);
                return;
            }

            // If the argument for the no-ui mode was not found we continue as a normal WPF app, creating the Application, initializing it and starting the message pump
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        public static void RunWithNoUI(string[] args)
        {
            // We need to attach a console to the process as it's not done by default when compiled as a WPF application
            AttachConsole(-1);

            try
            {
                Console.WriteLine($"Rayman Control Panel has started in no-ui mode");

                // At least 2 arguments are required (-noui + the command)
                if (args.Length < 2)
                {
                    Console.WriteLine($"Invalid argument count, has to be greater or equal to 2. The app will now close.");
                    return;
                }

                int pos = 1;

                // Multiple commands can be chained together, parse until we reach the end
                while (pos < args.Length)
                    ProcessCommand(args, ref pos);

                Console.WriteLine("All commands have been processed");
            }
            finally
            {
                // Make sure to free the console when we exit, even if an exception was thrown
                FreeConsole();
            }
        }

        public static void ProcessCommand(string[] args, ref int pos)
        {
            var cmd = ReadArg(args, ref pos);

            // Check the command
            switch (cmd)
            {
                case "test":
                    Console.WriteLine($"Test command has been processed");
                    break;

                case "wait":
                    var time = Int32.Parse(ReadArg(args, ref pos));

                    Console.WriteLine($"Waiting {time} ms");

                    Thread.Sleep(time);
                    break;

                default:
                    Console.WriteLine($"{cmd} is not a valid command");
                    break;
            }
        }

        public static string ReadArg(string[] args, ref int pos)
        {
            if (pos >= args.Length)
                throw new Exception("Requested argument was not included");

            return args[pos++];
        }

        [DllImport("Kernel32.dll")]
        public static extern bool AttachConsole(int processId);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();
    }
}