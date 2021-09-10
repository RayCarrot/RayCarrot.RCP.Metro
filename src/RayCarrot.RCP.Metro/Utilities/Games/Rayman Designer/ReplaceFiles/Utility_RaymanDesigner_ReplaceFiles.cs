using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Designer replace files utility
    /// </summary>
    public class Utility_RaymanDesigner_ReplaceFiles : IUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Utility_RaymanDesigner_ReplaceFiles()
        {
            ViewModel = new Utility_RaymanDesigner_ReplaceFiles_ViewModel();
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => Resources.RDU_ReplaceFilesHeader;

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => Resources.RDU_ReplaceFilesInfo;

        /// <summary>
        /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string WarningText => null;

        /// <summary>
        /// Indicates if the utility requires additional files to be downloaded remotely
        /// </summary>
        public bool RequiresAdditionalFiles => true;

        /// <summary>
        /// Indicates if the utility is work in process
        /// </summary>
        public bool IsWorkInProcess => false;

        /// <summary>
        /// The utility UI content
        /// </summary>
        public object UIContent => new Utility_RaymanDesigner_ReplaceFiles_UI()
        {
            DataContext = ViewModel
        };

        /// <summary>
        /// Indicates if the utility requires administration privileges
        /// </summary>
        public bool RequiresAdmin => !Services.File.CheckDirectoryWriteAccess(Games.RaymanDesigner.GetInstallDir());

        /// <summary>
        /// Indicates if the utility is available to the user
        /// </summary>
        public bool IsAvailable => Games.RaymanDesigner.GetInstallDir(false).DirectoryExists;

        /// <summary>
        /// Retrieves a list of applied utilities from this utility
        /// </summary>
        /// <returns>The applied utilities</returns>
        public IEnumerable<string> GetAppliedUtilities()
        {
            // Due to the changes not being able to be reverted this is not considered an applied utility
            return new string[0];
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public Utility_RaymanDesigner_ReplaceFiles_ViewModel ViewModel { get; }

        #endregion
    }
}