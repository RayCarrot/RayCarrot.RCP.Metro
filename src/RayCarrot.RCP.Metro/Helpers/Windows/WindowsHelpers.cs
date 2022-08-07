#nullable disable
using IWshRuntimeLibrary;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Helper methods for managing Windows specific requests
/// </summary>
public static class WindowsHelpers
{
    /// <summary>
    /// Returns true if the current program is running as administrator
    /// </summary>
    public static bool RunningAsAdmin => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    /// <summary>
    /// Opens Windows Explorer in specified path. Supports files and directories.
    /// </summary>
    /// <param name="path">Path to open</param>
    public static void OpenExplorerPath(string path)
    {
        if (System.IO.File.Exists(path))
            Process.Start("explorer.exe", "/select, \"" + path + "\"")?.Dispose();
        else
            Process.Start(path)?.Dispose();
    }

    /// <summary>
    /// Opens a specified path in RegEdit
    /// </summary>
    /// <param name="path">The path to open</param>
    public static void OpenRegistryPath(string path)
    {
        // Set the last opened path to the specified path so that will be opened on launch
        RunCommandPromptScript($@"REG ADD {AppFilePaths.RegeditRegistryKey} /v LastKey /t REG_SZ /d ""{path}"" /f");

        // Launch the program
        Process.Start("regedit")?.Dispose();
    }

    /// <summary>
    /// Runs a command prompt script as a new process
    /// </summary>
    /// <param name="script">The script to run</param>
    /// <param name="elevated">True if the script should be run in elevated mode</param>
    public static void RunCommandPromptScript(string script, bool elevated = false)
    {
        new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/c" + script,
                Verb = elevated ? "runas" : null
            }
        }.RunAndDispose(x =>
        {
            x.Start();

            x.WaitForExit();
        });
    }

    /// <summary>
    /// Creates a shortcut for a file
    /// </summary>
    /// <param name="shortcutName">The name of the shortcut file</param>
    /// <param name="destinationDirectory">The destination of the shortcut file</param>
    /// <param name="targetFile">The file the shortcut targets</param>
    /// <param name="arguments">Optional launch arguments</param>
    public static void CreateFileShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory, FileSystemPath targetFile, string arguments = null)
    {
        IWshShortcut shortcut = (IWshShortcut)new WshShell().CreateShortcut(destinationDirectory + shortcutName.ChangeFileExtension(new FileExtension(".lnk")));

        shortcut.TargetPath = targetFile;

        if (arguments != null)
            shortcut.Arguments = arguments;

        shortcut.WorkingDirectory = targetFile.Parent;

        shortcut.Save();
    }

    /// <summary>
    /// Creates a shortcut for an URL
    /// </summary>
    /// <param name="shortcutName">The name of the shortcut file</param>
    /// <param name="destinationDirectory">The path of the directory</param>
    /// <param name="targetURL">The URL</param>
    public static void CreateURLShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory, string targetURL)
    {
        using StreamWriter writer = new StreamWriter(destinationDirectory + shortcutName.ChangeFileExtension(new FileExtension(".url")));

        writer.WriteLine("[InternetShortcut]");
        writer.WriteLine("URL=" + targetURL);
            
        writer.Flush();
    }

    /// <summary>
    /// Gets the target arguments of a shortcut
    /// </summary>
    /// <param name="shortcutPath">The shortcut file</param>
    /// <returns></returns>
    public static string GetShortCutArguments(FileSystemPath shortcutPath) =>
        GetShortCutTargetInfo(shortcutPath).Arguments;

    /// <summary>
    /// Gets the target file/directory of a shortcut
    /// </summary>
    /// <param name="shortcutPath">The shortcut file</param>
    /// <returns></returns>
    public static string GetShortCutTarget(FileSystemPath shortcutPath) =>
        GetShortCutTargetInfo(shortcutPath).TargetPath;

    /// <summary>
    /// Gets the target info of a shortcut
    /// </summary>
    /// <param name="shortcutPath">The shortcut file</param>
    /// <returns>The target info</returns>
    public static IWshShortcut GetShortCutTargetInfo(FileSystemPath shortcutPath) =>
        ((IWshShortcut)new WshShell().CreateShortcut(shortcutPath));

    /// <summary>
    /// Gets the current Windows version
    /// </summary>
    /// <remarks>
    /// If the assembly manifest doesn't explicitly state that it is compatible with
    /// Windows 8.1 and 10 this will return the wrong Windows version
    /// </remarks>
    /// <returns>The Windows version</returns>
    public static WindowsVersion GetCurrentWindowsVersion()
    {
        var version = Environment.OSVersion;

        if (version.Platform == PlatformID.Win32Windows)
        {
            if (version.Version.Minor == 0)
                return WindowsVersion.Win95;

            if (version.Version.Minor == 10)
                return WindowsVersion.Win98;

            if (version.Version.Minor == 90)
                return WindowsVersion.WinMe;
        }
        else if (version.Platform == PlatformID.Win32NT)
        {
            if (version.Version.Major == 4 && version.Version.Minor == 0)
                return WindowsVersion.WinNT4;

            if (version.Version.Major == 5)
            {
                if (version.Version.Minor == 0)
                    return WindowsVersion.Win2000;

                if (version.Version.Minor == 1)
                    return WindowsVersion.WinXP;

                if (version.Version.Minor == 2)
                    return WindowsVersion.Win2003;
            }

            if (version.Version.Major == 6)
            {
                if (version.Version.Minor == 0)
                    return WindowsVersion.WinVista;

                if (version.Version.Minor == 1)
                    return WindowsVersion.Win7;

                if (version.Version.Minor == 2)
                    return WindowsVersion.Win8;

                if (version.Version.Minor == 3)
                    return WindowsVersion.Win81;
            }

            if (version.Version.Major == 10 && version.Version.Minor == 0)
                return WindowsVersion.Win10;
        }
        else
        {
            throw new PlatformNotSupportedException($"The current Windows version can not be retrieved on the platform {version.Platform}");
        }

        return WindowsVersion.Unknown;
    }

    /// <summary>
    /// Marks a file or directory to be deleted on reboot. This requires administration privileges.
    /// </summary>
    /// <param name="targetPath">The target to remove on reboot</param>
    /// <exception cref="Win32Exception"/>
    public static void DeleteOnReboot(FileSystemPath targetPath)
    {
        if (!MoveFileEx(targetPath, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT))
            throw new Win32Exception();
    }

    /// <summary>
    /// Finds the executable to use when opening a file
    /// </summary>
    /// <param name="filePath">The file to check</param>
    /// <param name="errorCode">The error code in case the operation fails</param>
    /// <returns>The executable path, or null if none was found</returns>
    public static string FindExecutableForFile(FileSystemPath filePath, out uint? errorCode)
    {
        var executable = new StringBuilder(1024);
            
        var m = FindExecutable(filePath, String.Empty, executable);

        if (m <= 32)
        {
            errorCode = m;

            return null;
        }

        var result = executable.ToString();

        errorCode = null;

        return result.IsNullOrEmpty() ? null : result;
    }

    /// <summary>
    /// Gets the icon or thumbnail for a file or directory
    /// </summary>
    /// <param name="path">The path of the file or directory to get the icon or thumbnail for</param>
    /// <param name="shellThumbnailSize">The size of the icon or thumbnail</param>
    /// <param name="getIcon">True if the icon should be returned or false if the thumbnail should be returned</param>
    /// <returns>The icon or thumbnail</returns>
    public static Bitmap GetIconOrThumbnail(string path, ShellThumbnailSize shellThumbnailSize, bool getIcon = true)
    {
        using var shellObject = ShellObject.FromParsingName(path);

        var thumb = shellObject.Thumbnail;
        thumb.FormatOption = getIcon ? ShellThumbnailFormatOption.IconOnly : ShellThumbnailFormatOption.ThumbnailOnly;
        return thumb.GetTransparentBitmap(shellThumbnailSize);
    }

    /// <summary>
    /// Sets the program associated with a file type
    /// </summary>
    /// <param name="programFilePath">The program file path</param>
    /// <param name="fileExtension">The file extension, including the period</param>
    /// <param name="description">The file type description</param>
    /// <param name="id">The ID</param>
    /// <param name="enable">Indicates if the file type association should be set. If false it is removed.</param>
    public static void SetFileTypeAssociation(FileSystemPath programFilePath, string fileExtension, string description, string id, bool enable)
    {
        if (enable)
        {
            Registry.SetValue(@$"HKEY_CURRENT_USER\Software\Classes\{fileExtension}", null, id);
            Registry.SetValue(@$"HKEY_CURRENT_USER\Software\Classes\{id}", null, description);
            Registry.SetValue(@$"HKEY_CURRENT_USER\Software\Classes\{id}\shell\open\command", null, $"\"{programFilePath.FullPath}\" \"%1\"");
        }
        else
        {
            Registry.CurrentUser.DeleteSubKeyTree(@$"Software\Classes\{fileExtension}");
            Registry.CurrentUser.DeleteSubKeyTree(@$"Software\Classes\{id}");
        }

        // Notify explorer of the change
        SHChangeNotify(
            // SHCNE_ASSOCCHANGED
            0x8000000,
            // SHCNF_FLUSH
            0x1000, IntPtr.Zero, IntPtr.Zero);
    }

    public static void SetURIProtocolAssociation(FileSystemPath programFilePath, string uriProtocol, string name, bool enable)
    {
        if (enable)
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(@$"Software\Classes\{uriProtocol}")!;

            key.SetValue(null, $"URL:{name}");
            key.SetValue("URL Protocol", String.Empty);

            using (RegistryKey iconKey = key.CreateSubKey("DefaultIcon")!)
                iconKey.SetValue(null, $"{programFilePath.FullPath},1");

            using (RegistryKey cmdKey = key.CreateSubKey(@"shell\open\command")!)
                cmdKey.SetValue(null, $"\"{programFilePath.FullPath}\" \"%1\"");
        }
        else
        {
            Registry.CurrentUser.DeleteSubKeyTree(@$"Software\Classes\{uriProtocol}");
        }
    }

    public static string GetFileTypeAssociationID(string fileExtension)
    {
        return Registry.GetValue(@$"HKEY_CURRENT_USER\Software\Classes\{fileExtension}", null, null) as string;
    }

    public static bool GetHasURIProtocolAssociation(string uriProtocol)
    {
        return RegistryHelpers.ValueExists(@$"HKEY_CURRENT_USER\Software\Classes\{uriProtocol}", "URL Protocol");
    }

    [Flags]
    private enum MoveFileFlags
    {
        //MOVEFILE_REPLACE_EXISTING = 1,
        //MOVEFILE_COPY_ALLOWED = 2,

        /// <summary>
        /// This value can be used only if the process is in the context of a user who belongs to the administrators group or the LocalSystem account
        /// </summary>
        MOVEFILE_DELAY_UNTIL_REBOOT = 4,
        //MOVEFILE_WRITE_THROUGH = 8
    }

    /// <summary>
    /// Marks the file for deletion during next system reboot
    /// </summary>
    /// <param name="lpExistingFileName">The current name of the file or directory on the local computer.</param>
    /// <param name="lpNewFileName">The new name of the file or directory on the local computer.</param>
    /// <param name="dwFlags">MoveFileFlags</param>
    /// <returns>bool</returns>
    /// <remarks>http://msdn.microsoft.com/en-us/library/aa365240(VS.85).aspx</remarks>
    [DllImport("kernel32.dll", EntryPoint = "MoveFileEx", SetLastError = true)]
    private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);

    [DllImport("shell32.dll", EntryPoint = "FindExecutable", SetLastError = true)]
    private static extern uint FindExecutable(string lpFile, string lpDirectory, StringBuilder lpResult);

    [DllImport("Shell32.dll", SetLastError = true)]
    private static extern void SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);
}