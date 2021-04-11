using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base Rayman 1 archive explorer utility
    /// </summary>
    public abstract class BaseRay1ExplorerUtility : IUtility
    {
        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => "Archive Explorer™ (.dat)"; // TODO-UPDATE: Localize
        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => "This tool allows you to view/edit the .dat archive files, allowing importing/exporting of game files"; // TODO-UPDATE: Localize

        /// <summary>
        /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string WarningText => null;

        /// <summary>
        /// Indicates if the utility requires additional files to be downloaded remotely
        /// </summary>
        public bool RequiresAdditionalFiles => false;

        /// <summary>
        /// Indicates if the utility is work in process
        /// </summary>
        public bool IsWorkInProcess => false;

        /// <summary>
        /// The utility UI content
        /// </summary>
        public object UIContent => new BaseArchiveExplorerGameUtilityUI()
        {
            DataContext = ViewModel
        };

        /// <summary>
        /// Indicates if the utility requires administration privileges
        /// </summary>
        public bool RequiresAdmin => false;

        /// <summary>
        /// Indicates if the utility is available to the user
        /// </summary>
        public bool IsAvailable => ViewModel.ArchiveFiles.Any(x => x.FileExists);

        /// <summary>
        /// Retrieves a list of applied utilities from this utility
        /// </summary>
        /// <returns>The applied utilities</returns>
        public IEnumerable<string> GetAppliedUtilities()
        {
            return new string[0];
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public abstract BaseRay1ArchiveExplorerUtilityViewModel ViewModel { get; }

        #endregion

        #region Public Methods

        public FileSystemPath[] GetArchiveFiles(FileSystemPath gameDir)
        {
            var nonArchiveDatFiles = new string[]
            {
                "ALLFIX.DAT",
                "BIGRAY.DAT",
                "CONCLU.DAT",
                "INTRO.DAT",
            };

            return Directory.GetFiles(gameDir, "*.dat", SearchOption.AllDirectories).Where(x => !nonArchiveDatFiles.Any(f => Path.GetFileName(x).Equals(f, StringComparison.OrdinalIgnoreCase))).Select(x => new FileSystemPath(x)).ToArray();
        }

        #endregion
    }
}