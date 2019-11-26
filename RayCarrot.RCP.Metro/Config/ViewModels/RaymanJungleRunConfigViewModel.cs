using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Jungle Run configuration
    /// </summary>
    public class RaymanJungleRunConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanJungleRunConfigViewModel()
        {
            // Create commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);

            // Create properties
            AsyncLock = new AsyncLock();
            SaveDir = SaveDir = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + Games.RaymanJungleRun.GetManager<RCPWinStoreGame>().FullPackageName + "LocalState";
        }

        #endregion

        #region Private Constants

        private const string SelectedHeroFileName = "ROHeroe";

        private const string SelectedSlotFileName = "ROselectedSlot";

        private const string SelectedVolumeFileName = "ROvolume";

        #endregion

        #region Private Fields

        private JungleRunHeroes _selectedHero;

        private byte _selectedSlot;

        private byte _musicVolume;
        
        private byte _soundVolume;

        #endregion

        #region Protected Properties

        /// <summary>
        /// The async lock to use for saving the configuration
        /// </summary>
        protected AsyncLock AsyncLock { get; }

        /// <summary>
        /// The save directory
        /// </summary>
        protected FileSystemPath SaveDir { get; }

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

        /// <summary>
        /// The music volume, a value between 0 and 100
        /// </summary>
        public byte MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The sound volume, a value between 0 and 100
        /// </summary>
        public byte SoundVolume
        {
            get => _soundVolume;
            set
            {
                _soundVolume = value;
                UnsavedChanges = true;
            }
        }

        #endregion

        #region Commands

        public AsyncRelayCommand SaveCommand { get; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Reads a single byte from the specified file relative to the current save data
        /// </summary>
        /// <param name="fileName">The file name, relative to the current save data</param>
        /// <returns>The byte or null if not found</returns>
        protected byte? ReadSingleByteFile(FileSystemPath fileName)
        {
            return ReadMultiByteFile(fileName, 1)?.FirstOrDefault();
        }

        /// <summary>
        /// Reads multiple bytes from the specified file relative to the current save data
        /// </summary>
        /// <param name="fileName">The file name, relative to the current save data</param>
        /// <param name="length">The amount of bytes to read</param>
        /// <returns>The bytes or null if not found</returns>
        protected byte[] ReadMultiByteFile(FileSystemPath fileName, int length)
        {
            // Get the file path
            var filePath = SaveDir + fileName;

            // Make sure the file exists
            if (!filePath.FileExists)
                return null;

            // Create the file stream
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Create the byte buffer
            var buffer = new byte[length];

            // Read the bytes
            stream.Read(buffer, 0, length);

            // Return the buffer
            return buffer;
        }

        /// <summary>
        /// Writes a single byte to the specified file relative to the current save data
        /// </summary>
        /// <param name="fileName">The file name, relative to the current save data</param>
        /// <param name="value">The byte to write</param>
        protected void WriteSingleByteFile(FileSystemPath fileName, byte value)
        {
            WriteMultiByteFile(fileName, new byte[]
            {
                value
            });
        }

        /// <summary>
        /// Writes multiple bytes to the specified file relative to the current save data
        /// </summary>
        /// <param name="fileName">The file name, relative to the current save data</param>
        /// <param name="value">The bytes to write</param>
        protected void WriteMultiByteFile(FileSystemPath fileName, byte[] value)
        {
            // Get the file path
            var filePath = SaveDir + fileName;

            // Create the file stream
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

            // Write the bytes
            stream.Write(value, 0, value.Length);
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