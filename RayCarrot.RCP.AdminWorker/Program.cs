using System;
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
                    MessageBox.Show("The grantFullControl requires a second argument specifying the file path", "Invalid launch context", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
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