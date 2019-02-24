using RayCarrot.CarrotFramework;
using System;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// An item for a game installation
    /// </summary>
    public class RayGameInstallItem
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="relativeSourcePath">The relative source path</param>
        /// <param name="outputPath">The output path</param>
        /// <param name="optional">Indicates if the item is optional</param>
        public RayGameInstallItem(FileSystemPath relativeSourcePath, FileSystemPath outputPath, bool optional = false)
        {
            RelativeSourcePath = relativeSourcePath;
            OutputPath = outputPath;
            Optional = optional;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The relative source path
        /// </summary>
        public FileSystemPath RelativeSourcePath { get; }

        /// <summary>
        /// The output path
        /// </summary>
        public FileSystemPath OutputPath { get; }

        /// <summary>
        /// The absolute input path
        /// </summary>
        public FileSystemPath InputPath => BasePath + RelativeSourcePath;

        /// <summary>
        /// The base path for the input
        /// </summary>
        public FileSystemPath BasePath { get; private set; }

        /// <summary>
        /// The base path's drive's label
        /// </summary>
        public string BaseDriveLabel { get; private set; }

        /// <summary>
        /// True if the item is optional, false if not
        /// </summary>
        public bool Optional { get; }

        /// <summary>
        /// The process stage of the current item
        /// </summary>
        public RayGameInstallItemStage ProcessStage { get; set; } = RayGameInstallItemStage.Initial;

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the base path if the absolute path is found
        /// </summary>
        /// <param name="basePath">The base path to add</param>
        /// <returns>True if it was added, false if not</returns>
        public bool AddIfExists(FileSystemPath basePath)
        {
            try
            {
                if (!(basePath + RelativeSourcePath).Exists)
                    return false;

                BaseDriveLabel = new DriveInfo(basePath).VolumeLabel;
                BasePath = basePath;
                return true;
            }
            catch (Exception ex)
            {
                ex.HandleError("Setting game install item base path");
                return false;
            }
        }

        #endregion
    }
}