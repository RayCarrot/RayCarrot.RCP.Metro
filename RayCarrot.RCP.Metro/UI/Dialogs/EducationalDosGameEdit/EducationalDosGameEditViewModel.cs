using System.IO;
using System.Linq;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for editing an educational DOS game
    /// </summary>
    public class EducationalDosGameEditViewModel : UserInputViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to edit</param>
        public EducationalDosGameEditViewModel(UserData_EducationalDosBoxGameData game)
        {
            // Get the available launch modes
            AvailableLaunchModes = Directory.GetDirectories(game.InstallDir + "PCMAP", "*", SearchOption.TopDirectoryOnly).Select(x => new FileSystemPath(x).Name).ToArray();

            MountPath = game.MountPath;
            LaunchMode = game.LaunchMode ?? AvailableLaunchModes.FirstOrDefault();
            Name = game.Name;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected mount path
        /// </summary>
        public FileSystemPath MountPath { get; set; }

        /// <summary>
        /// The available launch modes
        /// </summary>
        public string[] AvailableLaunchModes { get; }

        /// <summary>
        /// The selected launch mode
        /// </summary>
        public string LaunchMode { get; set; }

        /// <summary>
        /// The selected name
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}