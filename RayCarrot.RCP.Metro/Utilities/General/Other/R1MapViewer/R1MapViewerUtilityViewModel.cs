﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.Rayman.Rayman1;
using RayCarrot.UI;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for viewing Rayman 1 maps
    /// </summary>
    public class R1MapViewerUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R1MapViewerUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<Rayman1GameMode>(Rayman1GameMode.Rayman1PC, new Rayman1GameMode[]
            {
                Rayman1GameMode.Rayman1PC,
            });

            LoadR1LevelCommand = new AsyncRelayCommand(LoadR1LevelAsync);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game mode selection for .lev files
        /// </summary>
        public EnumSelectionViewModel<Rayman1GameMode> GameModeSelection { get; }

        /// <summary>
        /// The currently loaded image source for a .lev graphic
        /// </summary>
        public ImageSource LevGraphicsImageSource { get; set; }

        /// <summary>
        /// The currently loaded image source for a .lev type graphic
        /// </summary>
        public ImageSource LevTypesImageSource { get; set; }

        /// <summary>
        /// The currently loaded .lev data
        /// </summary>
        public Rayman1PCLevData PcLevData { get; set; }

        /// <summary>
        /// Indicates if the utility is loading
        /// </summary>
        public bool IsLoading { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads a Rayman 1 level map
        /// </summary>
        /// <returns>The task</returns>
        public async Task LoadR1LevelAsync()
        {
            // Allow the user to select the level file
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_R1MapViewer_SelectLevel,
                DefaultDirectory = GameModeSelection.SelectedValue.GetGame()?.GetInstallDir(false).FullPath,
                ExtensionFilter = new FileFilterItem("*.lev", "LEV").ToString(),
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            try
            {
                IsLoading = true;

                LevGraphicsImageSource = null;
                LevTypesImageSource = null;

                // Get the level data
                PcLevData = await Task.Run(() => Rayman1PCLevData.GetSerializer().Deserialize(fileResult.SelectedFile));

                LevGraphicsImageSource = (await Task.Run(() => PcLevData.GetBitmap())).ToImageSource();
                LevTypesImageSource = (await Task.Run(() => PcLevData.GetTypeBitmap())).ToImageSource();
            }
            catch (Exception ex)
            {
                ex.HandleError("Loading R1 level map");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_R1MapViewer_Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Commands

        public ICommand LoadR1LevelCommand { get; }

        #endregion
    }
}