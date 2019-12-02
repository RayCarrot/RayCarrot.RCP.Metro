using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Jungle Run configuration
    /// </summary>
    public class RaymanJungleRunConfigViewModel : BaseRayRunConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanJungleRunConfigViewModel() : base(Games.RaymanJungleRun)
        {
            // Create commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        #endregion

        #region Commands

        public AsyncRelayCommand SaveCommand { get; }

        #endregion

        #region Private Constants

        private const string SelectedHeroFileName = "ROHeroe";

        private const string SelectedSlotFileName = "ROselectedSlot";

        private const string SelectedVolumeFileName = "ROvolume";

        #endregion

        #region Private Fields

        private JungleRunHeroes _selectedHero;

        private byte _selectedSlot;

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected hero
        /// </summary>
        public JungleRunHeroes SelectedHero
        {
            get => _selectedHero;
            set
            {
                _selectedHero = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected save slot, a value between 0 and 2
        /// </summary>
        public byte SelectedSlot
        {
            get => _selectedSlot;
            set
            {
                _selectedSlot = value;
                UnsavedChanges = true;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public override Task SetupAsync()
        {
            RCFCore.Logger?.LogInformationSource("Rayman Jungle Run config is being set up");

            // Read selected hero and save slot data
            SelectedHero = (JungleRunHeroes)(ReadSingleByteFile(SelectedHeroFileName) ?? 0);
            SelectedSlot = ReadSingleByteFile(SelectedSlotFileName) ?? 0;

            // Read volume
            var ROvolume = ReadMultiByteFile(SelectedVolumeFileName, 2);
            MusicVolume = ROvolume?[0] ?? 100;
            SoundVolume = ROvolume?[1] ?? 100;

            UnsavedChanges = false;

            RCFCore.Logger?.LogInformationSource($"All values have been loaded");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                RCFCore.Logger?.LogInformationSource($"Rayman Jungle Run configuration is saving...");

                try
                {
                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(SaveDir);

                    // Save selected hero and save slot data
                    WriteSingleByteFile(SelectedHeroFileName, (byte)SelectedHero);
                    WriteSingleByteFile(SelectedSlotFileName, SelectedSlot);

                    // Save the volume
                    WriteMultiByteFile(SelectedVolumeFileName, new byte[]
                    {
                        MusicVolume,
                        SoundVolume
                    });

                    RCFCore.Logger?.LogInformationSource($"Rayman Jungle Run configuration has been saved");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving Rayman Jungle Run config");
                    await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, Games.RaymanJungleRun.GetGameInfo().DisplayName), Resources.Config_SaveErrorHeader);
                    return;
                }

                UnsavedChanges = false;

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Config_SaveSuccess);

                OnSave();
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// The available heroes in Rayman Jungle Run
        /// </summary>
        public enum JungleRunHeroes
        {
            Rayman,
            Globox,
            DarkRayman,
            GloboxOutfit,
            GreenTeensy,
            GothTeensy
        }

        #endregion
    }
}