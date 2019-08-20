using System;
using System.Text.RegularExpressions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Game info for an educational DOSBox game
    /// </summary>
    public class EducationalDosBoxGameInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="id">The game ID or null to generate a new one</param>
        /// <param name="installDIr">The game install directory</param>
        /// <param name="launchName">The launch name</param>
        public EducationalDosBoxGameInfo(string id, FileSystemPath installDIr, string launchName)
        {
            ID = id ?? Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            InstallDIr = installDIr;
            LaunchName = launchName;
            Name = String.Empty;
            MountPath = FileSystemPath.EmptyPath;
        }

        /// <summary>
        /// The game ID
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The launch name
        /// </summary>
        public string LaunchName { get; }

        /// <summary>
        /// The game name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The game install directory
        /// </summary>
        public FileSystemPath InstallDIr { get; }

        /// <summary>
        /// The mount path
        /// </summary>
        public FileSystemPath MountPath { get; set; }
    }
}