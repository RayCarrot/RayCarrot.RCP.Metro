using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base for Rayman Jungle/Fiesta Run config view model
    /// </summary>
    public class BaseRayRunConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        protected BaseRayRunConfigViewModel(Games game)
        {
            // Create properties
            AsyncLock = new AsyncLock();
            Game = game;

            // Create commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        #endregion

        #region Private Constants

        private const string SelectedVolumeFileName = "ROvolume";

        #endregion

        #region Private Fields

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
        protected FileSystemPath SaveDir { get; set; }

        /// <summary>
        /// The game
        /// </summary>
        protected Games Game { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The music volume, a value between 0 and 99
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
        /// The sound volume, a value between 0 and 99
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
        protected virtual byte? ReadSingleByteFile(FileSystemPath fileName)
        {
            return ReadMultiByteFile(fileName, 1)?.FirstOrDefault();
        }

        /// <summary>
        /// Reads multiple bytes from the specified file relative to the current save data
        /// </summary>
        /// <param name="fileName">The file name, relative to the current save data</param>
        /// <param name="length">The amount of bytes to read</param>
        /// <returns>The bytes or null if not found</returns>
        protected virtual byte[] ReadMultiByteFile(FileSystemPath fileName, int length)
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
        protected virtual void WriteSingleByteFile(FileSystemPath fileName, byte value)
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
        protected virtual void WriteMultiByteFile(FileSystemPath fileName, byte[] value)
        {
            // Get the file path
            var filePath = SaveDir + fileName;

            // Create the file stream
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

            // Write the bytes
            stream.Write(value, 0, value.Length);
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Sets up the game specific values
        /// </summary>
        /// <returns>The task</returns>
        protected virtual Task SetupGameAsync() => Task.CompletedTask;

        /// <summary>
        /// Saves the game specific values
        /// </summary>
        /// <returns>The task</returns>
        protected virtual Task SaveGameAsync() => Task.CompletedTask;

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public override async Task SetupAsync()
        {
            RCFCore.Logger?.LogInformationSource($"{Game} config is being set up");

            // Get the save directory
            SaveDir = SaveDir = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + Game.GetManager<RCPWinStoreGame>().FullPackageName + "LocalState";

            // Read game specific values
            await SetupGameAsync();

            // Read volume
            var ROvolume = ReadMultiByteFile(SelectedVolumeFileName, 2);
            MusicVolume = ROvolume?[0] ?? 100;
            SoundVolume = ROvolume?[1] ?? 100;

            UnsavedChanges = false;

            RCFCore.Logger?.LogInformationSource($"All values have been loaded");
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                RCFCore.Logger?.LogInformationSource($"{Game} configuration is saving...");

                try
                {
                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(SaveDir);

                    // Save game specific values
                    await SaveGameAsync();

                    // Save the volume
                    WriteMultiByteFile(SelectedVolumeFileName, new byte[]
                    {
                        MusicVolume,
                        SoundVolume
                    });

                    RCFCore.Logger?.LogInformationSource($"{Game} configuration has been saved");
                }
                catch (Exception ex)
                {
                    ex.HandleError($"Saving {Game} config");
                    await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, Game.GetGameInfo().DisplayName), Resources.Config_SaveErrorHeader);
                    return;
                }

                UnsavedChanges = false;

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Config_SaveSuccess);

                OnSave();
            }
        }

        #endregion
    }
}