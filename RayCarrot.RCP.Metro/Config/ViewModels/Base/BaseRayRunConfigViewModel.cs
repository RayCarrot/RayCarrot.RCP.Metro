using System;
using System.IO;
using System.Linq;
using Nito.AsyncEx;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base for Rayman Jungle/Fiesta Run config view model
    /// </summary>
    public abstract class BaseRayRunConfigViewModel : GameConfigViewModel
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
            SaveDir = SaveDir = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + game.GetManager<RCPWinStoreGame>().FullPackageName + "LocalState";
        }

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
        protected FileSystemPath SaveDir { get; }

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
    }
}