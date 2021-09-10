using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace RayCarrot.RCP.Uninstaller
{
    /// <summary>
    /// The program
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The main entry point
        /// </summary>
        /// <param name="args">The launch arguments</param>
        private static void Main(string[] args)
        {
            // Get the program file path
            string rcpFilePath = args.Any() ? args.First() : null;

            // Make sure the program path exists
            if (rcpFilePath == null || !File.Exists(rcpFilePath))
            {
                Console.WriteLine("The main program file was not found. Do you want to continue removing program app data? (Y/N)");
                if (Console.ReadLine()?.Equals("Y", StringComparison.InvariantCultureIgnoreCase) != true)
                    return;
            }

            int numberOfTries = 0;
            const int maxNumberOfTries = 20;

            // Make sure the program is not running, and if so attempt to wait for it to exit
            while (IsFileLocked(rcpFilePath))
            {
                numberOfTries++;
                Console.WriteLine($"The main program file is currently not available. Retrying {numberOfTries}/{maxNumberOfTries}");

                if (numberOfTries == maxNumberOfTries)
                {
                    Console.WriteLine("Uninstallation failed: The main program file is currently not available.");

                    Console.ReadLine();
                    return;
                }

                Thread.Sleep(500);
            }

            // Get the user paths
            string uninstallerFile = Assembly.GetEntryAssembly()?.Location;
            string rcpUserDataBaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Rayman Control Panel");
            string rcpUserDataDir = Path.Combine(rcpUserDataBaseDir, "RCP_Metro");

            // Make sure the uninstaller is located in the user data base directory
            if (!uninstallerFile.Contains(rcpUserDataDir))
            {
                Console.WriteLine("Uninstallation failed: The uninstaller has not been deployed correctly");

                Console.ReadLine();
                return;
            }

            var dirsToDeleteOnReboot = new List<string>();

            string tempUninstallerFilePath = uninstallerFile;

            while (tempUninstallerFilePath != rcpUserDataDir)
            {
                tempUninstallerFilePath = Path.GetDirectoryName(tempUninstallerFilePath);
                dirsToDeleteOnReboot.Add(tempUninstallerFilePath);
            }

            // Add base directory to list if it only contains the RCP folder
            if (Directory.GetFileSystemEntries(rcpUserDataBaseDir, "*", SearchOption.TopDirectoryOnly).Length == 1)
                dirsToDeleteOnReboot.Add(rcpUserDataBaseDir);

            // Set the uninstaller to be deleted on reboot
            bool succeeded = DeleteOnReboot(uninstallerFile);

            if (succeeded)
            {
                Console.WriteLine("Marked to delete on reboot: " + uninstallerFile);

                // Set the user data to be deleted on reboot
                foreach (var dir in dirsToDeleteOnReboot)
                {
                    succeeded = DeleteOnReboot(dir);

                    if (!succeeded)
                        break;

                    Console.WriteLine("Marked to delete on reboot: " + dir);
                }
            }

            // Cancel if the previous process failed
            if (!succeeded)
            {
                Console.WriteLine("Uninstallation failed: An error occurred when setting the user data to remove on reboot");
                Console.WriteLine(new Win32Exception().ToString());

                Console.ReadLine();
                return;
            }

            try
            {
                // Delete every file in the user data besides the uninstaller
                foreach (string file in Directory.GetFiles(rcpUserDataDir, "*", SearchOption.AllDirectories))
                {
                    if (file != uninstallerFile)
                    {
                        File.Delete(file);
                        Console.WriteLine("Deleted: " + file);
                    }
                }

                // Delete every empty directory in the user data
                foreach (string directory in Directory.GetDirectories(rcpUserDataDir, "*", SearchOption.AllDirectories))
                {
                    if (Directory.Exists(directory) && !Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Any())
                    {
                        Directory.Delete(directory, true);
                        Console.WriteLine("Deleted: " + directory);
                    }
                }

                // Open Registry app data key
                using (var softwareKey = Registry.CurrentUser.OpenSubKey(@"Software", true))
                {
                    if (softwareKey == null)
                        throw new Exception("The software registry key was null");

                    // Make sure the RayCarrot key exists
                    if (softwareKey.GetSubKeyNames().Contains("RayCarrot"))
                    {
                        bool isRayCarrotKeyEmpty;

                        // Open the RayCarrot sub key
                        using (var rayCarrotKey = softwareKey.OpenSubKey("RayCarrot", true))
                        {
                            if (rayCarrotKey == null)
                                throw new Exception("The RayCarrot registry key was null");

                            // Delete the RCP sub key if it exists
                            if (rayCarrotKey.GetSubKeyNames().Contains("RCP_Metro"))
                            {
                                rayCarrotKey.DeleteSubKey(@"RCP_Metro");
                                Console.WriteLine("Deleted: " + rayCarrotKey.Name + @"\RCP_Metro");
                            }

                            // Check if the RayCarrot sub key is empty
                            isRayCarrotKeyEmpty = rayCarrotKey.SubKeyCount == 0 && rayCarrotKey.ValueCount == 0;
                        }

                        // Delete RayCarrot key if empty
                        if (isRayCarrotKeyEmpty)
                        {
                            softwareKey.DeleteSubKey("RayCarrot");
                            Console.WriteLine("Deleted: " + softwareKey.Name + @"\RayCarrot");
                        }
                    }
                }

                // Open Registry uninstall key
                using (var uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
                {
                    if (uninstallKey == null)
                        throw new Exception("The uninstall registry key was null");

                    // Delete the uninstall key for RCP if it exists
                    if (uninstallKey.GetSubKeyNames().Contains("RCP_Metro"))
                    {
                        uninstallKey.DeleteSubKey("RCP_Metro");
                        Console.WriteLine("Deleted: " + uninstallKey.Name + @"\RCP_Metro");
                    }
                }

                // Delete the program if it exists
                if (rcpFilePath != null && File.Exists(rcpFilePath))
                    File.Delete(rcpFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Uninstallation failed: An error occurred while removing app data. Some data may have been removed.");
                Console.WriteLine(ex.ToString());

                Console.ReadLine();
                return;
            }

            // Finish
            Console.WriteLine(String.Empty);
            Console.WriteLine(String.Empty);
            Console.WriteLine("The uninstaller finished successfully. Reboot your computer to finish the uninstallation.");
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Marks a file or directory to be deleted on reboot. This requires administration privileges.
        /// </summary>
        /// <param name="targetPath">The target to remove on reboot</param>
        /// <returns>True if it succeeded, otherwise false</returns>
        private static bool DeleteOnReboot(string targetPath) => MoveFileEx(targetPath, null, 4);

        /// <summary>
        /// Marks the file for deletion during next system reboot
        /// </summary>
        /// <param name="lpExistingFileName">The current name of the file or directory on the local computer.</param>
        /// <param name="lpNewFileName">The new name of the file or directory on the local computer.</param>
        /// <param name="dwFlags">MoveFileFlags</param>
        /// <returns>True if it succeeded, otherwise false</returns>
        [DllImport("kernel32.dll", EntryPoint = "MoveFileEx", SetLastError = true)]
        private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);

        /// <summary>
        /// Indicates if the file is locked
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>True if the file is locked and can not be written to, or false if it's not</returns>
        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}