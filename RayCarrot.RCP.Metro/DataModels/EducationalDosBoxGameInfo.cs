using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        /// <param name="installDir">The game install directory</param>
        /// <param name="launchName">The launch name</param>
        public EducationalDosBoxGameInfo(string id, FileSystemPath installDir, string launchName)
        {
            //ID = id ?? Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");

            // The ID is calculated based on the files so that it will be the same if the same game is added again
            ID = id ?? Games.EducationalDos.GetGameManager<EducationalDosBoxGameManager>().GetGameID(installDir, launchName);

            InstallDir = installDir;
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
        /// The launch mode argument
        /// </summary>
        public string LaunchMode { get; set; }

        /// <summary>
        /// The game install directory
        /// </summary>
        public FileSystemPath InstallDir { get; }

        /// <summary>
        /// The mount path
        /// </summary>
        public FileSystemPath MountPath { get; set; }
    }
}