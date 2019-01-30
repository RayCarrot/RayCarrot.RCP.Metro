using RayCarrot.CarrotFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Helpers methods for DosBox
    /// </summary>
    public static class DosBoxHelpers
    {
        /// <summary>
        /// Creates a DosBox argument for starting a game with specific options
        /// </summary>
        /// <param name="configPath">The path of the DosBox config file</param>
        /// <param name="installPath">Game install path</param>
        /// <param name="mountPath">The disc/file to mount</param>
        /// <param name="code">Code to send to DosBox before launching the game.</param>
        /// <param name="exe">The game executable file to launch</param>
        /// <returns></returns>
        public static string GetDosBoxArgument(FileSystemPath configPath, FileSystemPath installPath, FileSystemPath mountPath, IEnumerable<string> code, string exe)
        {
            string argument = String.Empty;

            // Add the config file
            if (configPath.FileExists)
                argument += "-conf " + configPath.FullPath + " ";
        
            // The mounting differs if it's a physical disc vs. a disc image
            if (mountPath.IsDirectoryRoot())
                argument += "-c \"mount d " + mountPath.FullPath + " -t cdrom\" ";
            else
                argument += "-c \"imgmount d '" + mountPath.FullPath + "' -t iso -fs iso\" ";

            //Adds user defined code to the argument
            argument = code.Where(line => !line.IsNullOrEmpty()).Aggregate(argument, (current, line) => current + "-c \"" + line.Replace('\"', '\'') + "\" ");

            argument += "-c \"MOUNT C '" + installPath.FullPath + "'\" -c C: -c \"" + exe + "\" -noconsole -c exit";

            return argument;
        }
    }
}