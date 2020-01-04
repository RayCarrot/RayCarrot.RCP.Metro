using RayCarrot.IO;
using RayCarrot.RCP.Core;
using System;
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The application user data
    /// </summary>
    public class AppUserData : RCPAppUserData
    {
        #region Public Override Methods

        /// <summary>
        /// Resets all values to their defaults
        /// </summary>
        public override void Reset()
        {
            // Call base
            base.Reset();

            // Reset properties
            Games = new Dictionary<Games, GameData>();
            DosBoxGames = new Dictionary<Games, DosBoxOptions>();
            IsFirstLaunch = true;
            DosBoxPath = FileSystemPath.EmptyPath;
            DosBoxConfig = FileSystemPath.EmptyPath;
            AutoLocateGames = true;
            ShowNotInstalledGames = true;
            CloseAppOnGameLaunch = false;
            CloseConfigOnSave = true;
            BackupLocation = Environment.SpecialFolder.MyDocuments.GetFolderPath();
            TPLSData = null;
            LinkItemStyle = LinkItemStyles.List;
            LinkListHorizontalAlignment = HorizontalAlignment.Left;
            CompressBackups = true;
            FiestaRunVersion = FiestaRunEdition.Default;
            EducationalDosBoxGames = null;
            RRR2LaunchMode = RRR2LaunchMode.AllGames;
            RabbidsGoHomeLaunchData = null;
            JumpListItemIDCollection = new List<string>();
            InstalledGames = new HashSet<Games>();
            CategorizeGames = true;
            ShownRabbidsActivityCenterLaunchMessage = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The saved games
        /// </summary>
        public Dictionary<Games, GameData> Games { get; set; }

        /// <summary>
        /// The DosBox games options
        /// </summary>
        public Dictionary<Games, DosBoxOptions> DosBoxGames { get; set; }

        /// <summary>
        /// Indicates if it is the first time the application launches
        /// </summary>
        public bool IsFirstLaunch { get; set; }

        /// <summary>
        /// The path for the DosBox file
        /// </summary>
        public string DosBoxPath { get; set; }

        /// <summary>
        /// Optional path for the DosBox config file
        /// </summary>
        public string DosBoxConfig { get; set; }

        /// <summary>
        /// Indicates if games should be automatically located on startup
        /// </summary>
        public bool AutoLocateGames { get; set; }

        /// <summary>
        /// Indicates if not installed games should be shown
        /// </summary>
        public bool ShowNotInstalledGames { get; set; }

        /// <summary>
        /// Indicates if the application should close when a game is launched
        /// </summary>
        public bool CloseAppOnGameLaunch { get; set; }

        /// <summary>
        /// Indicates if configuration dialogs should close upon saving
        /// </summary>
        public bool CloseConfigOnSave { get; set; }

        /// <summary>
        /// The backup directory path
        /// </summary>
        public FileSystemPath BackupLocation { get; set; }

        /// <summary>
        /// The current TPLS data if installed, otherwise null
        /// </summary>
        public TPLSData TPLSData { get; set; }

        /// <summary>
        /// The current link item style
        /// </summary>
        public LinkItemStyles LinkItemStyle { get; set; }

        /// <summary>
        /// The horizontal alignment for link items in list view
        /// </summary>
        public HorizontalAlignment LinkListHorizontalAlignment { get; set; }

        /// <summary>
        /// Indicates if backups should be compressed
        /// </summary>
        public bool CompressBackups { get; set; }

        /// <summary>
        /// Indicates the current version of Rayman Fiesta Run
        /// </summary>
        public FiestaRunEdition FiestaRunVersion { get; set; }

        /// <summary>
        /// The saved educational DOSBox games
        /// </summary>
        public List<EducationalDosBoxGameData> EducationalDosBoxGames { get; set; }

        /// <summary>
        /// The launch mode to use for Rayman Raving Rabbids 2
        /// </summary>
        public RRR2LaunchMode RRR2LaunchMode { get; set; }

        /// <summary>
        /// The launch data for Rabbids Go Home
        /// </summary>
        public RabbidsGoHomeLaunchData RabbidsGoHomeLaunchData { get; set; }

        /// <summary>
        /// The collection of jump list item IDs
        /// </summary>
        public List<string> JumpListItemIDCollection { get; set; }

        /// <summary>
        /// The games which have been installed through the Rayman Control Panel
        /// </summary>
        public HashSet<Games> InstalledGames { get; set; }

        /// <summary>
        /// Indicates if the games should be categorized
        /// </summary>
        public bool CategorizeGames { get; set; }

        /// <summary>
        /// Indicates if the launch message for Rayman Raving Rabbids Activity Center has been shown
        /// </summary>
        public bool ShownRabbidsActivityCenterLaunchMessage { get; set; }

        #endregion
    }
}