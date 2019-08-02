using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;

namespace RayCarrot.RCP.AdminWorker
{
    /// <summary>
    /// The application
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The entry point
        /// </summary>
        /// <param name="args">The launch arguments</param>
        internal static void Main(string[] args)
        {
            // Make sure we have an argument
            if (!args.Any())
            {
                MessageBox.Show("The admin worker could not run due to no launch arguments being found", "Invalid launch context", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (args.First().Equals("grantFullControl", StringComparison.InvariantCultureIgnoreCase))
            {
                var path = args.ElementAtOrDefault(1);

                if (File.Exists(path))
                    GrantFullControlToFile(path);
                else
                    MessageBox.Show("The grantFullControl operation requires a second argument specifying the file path", "Invalid launch context", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            else if (args.First().Equals("RestartAsAdmin", StringComparison.InvariantCultureIgnoreCase))
            {
                var id = args.ElementAtOrDefault(1);

                if (id == null)
                {
                    MessageBox.Show("The RestartAsAdmin operation requires a second argument specifying the process ID", "Invalid launch context", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                Process p;

                try
                {
                    // Get the process
                    p = Process.GetProcessById(Int32.Parse(id));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while finding process of ID: {id}.{Environment.NewLine}{Environment.NewLine}Error message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // Get the path
                var path = p.MainModule?.FileName;

                // Make sure the path exists
                if (path == null || !File.Exists(path))
                {
                    MessageBox.Show($"The process path is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                try
                {
                    // Close the main window
                    p.CloseMainWindow();

                    // Wait a maximum of 20 seconds for exit
                    p.WaitForExit(20000);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while exiting the process.{Environment.NewLine}{Environment.NewLine}Error message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                try
                {
                    // Start the process again
                    Process.Start(path);

                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while starting the process again.{Environment.NewLine}{Environment.NewLine}Error message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }

            // Show message if no valid arguments were found
            MessageBox.Show("The launch argument used is not valid for this version of the program", "Invalid launch context", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Grants full control to the specified file
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        private static void GrantFullControlToFile(string filePath)
        {
            try
            {
                // Get the current access control
                var data = File.GetAccessControl(filePath);

                // Add a new access rule to allow everyone full control of the file
                data.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(
                        // Use "WorldSid" to ensure every region gets this identifier
                        WellKnownSidType.WorldSid, null),
                    // Grant full control
                    FileSystemRights.FullControl,
                    // Files can't have inheritance...
                    InheritanceFlags.None,
                    PropagationFlags.None,
                    // Allow the full access
                    AccessControlType.Allow));

                // Set the modified access control
                File.SetAccessControl(filePath, data);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while granting full file control to the specified path: {filePath}.{Environment.NewLine}{Environment.NewLine}Error message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}