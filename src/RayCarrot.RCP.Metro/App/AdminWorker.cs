using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;

namespace RayCarrot.RCP.Metro;

public static class AdminWorker
{
    public const string MainArg = "-adminworker";
    public const string GrantFullFileControlArg = "-fullcontrol";

    private static void GrantFullControlToFile(string filePath)
    {
        try
        {
            FileInfo filePathInfo = new(filePath);

            // Get the current access control
            FileSecurity fileSecurity = filePathInfo.GetAccessControl();

            // Add a new access rule to allow everyone full control of the file
            fileSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(
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
            filePathInfo.SetAccessControl(fileSecurity);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while granting full file control to the specified path: {filePath}.{Environment.NewLine}{Environment.NewLine}Error message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void Run(LaunchArguments args)
    {
        if (args.HasArg(GrantFullFileControlArg, out string? fullControlFilePath))
            GrantFullControlToFile(fullControlFilePath);
    }
}