using System.IO;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// Base for Rayman Jungle/Fiesta Run config view model
/// </summary>
public abstract class UbiArtRunBaseConfigViewModel : ConfigPageViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameDescriptor">The game descriptor</param>
    /// <param name="gameInstallation">The game installation</param>
    protected UbiArtRunBaseConfigViewModel(WindowsPackageGameDescriptor gameDescriptor, GameInstallation gameInstallation)
    {
        // Set properties
        GameDescriptor = gameDescriptor;
        GameInstallation = gameInstallation;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Constants

    private const string SelectedVolumeFileName = "ROvolume";

    #endregion

    #region Private Fields

    private byte _musicVolume;

    private byte _soundVolume;

    #endregion

    #region Private Properties

    /// <summary>
    /// The save directory
    /// </summary>
    private FileSystemPath SaveDir { get; set; }

    private GameInstallation GameInstallation { get; }

    /// <summary>
    /// The game descriptor
    /// </summary>
    private WindowsPackageGameDescriptor GameDescriptor { get; }

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
    protected virtual byte[]? ReadMultiByteFile(FileSystemPath fileName, int length)
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

    /// <summary>
    /// Loads and sets up the current configuration properties
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task LoadAsync()
    {
        Logger.Info("{0} config is being set up", GameDescriptor.GameId);

        // Get the save directory
        SaveDir = GameDescriptor.GetLocalAppDataDirectory();

        // Read game specific values
        await SetupGameAsync();

        // Read volume
        var ROvolume = ReadMultiByteFile(SelectedVolumeFileName, 2);
        MusicVolume = ROvolume?[0] ?? 100;
        SoundVolume = ROvolume?[1] ?? 100;

        UnsavedChanges = false;

        Logger.Info("All values have been loaded");
    }

    /// <summary>
    /// Saves the changes
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task<bool> SaveAsync()
    {
        Logger.Info("{0} configuration is saving...", GameDescriptor.GameId);

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

            Logger.Info("{0} configuration has been saved", GameDescriptor.GameId);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Saving {0} config", GameDescriptor.GameId);
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, GameInstallation.GetDisplayName()), Resources.Config_SaveErrorHeader);
            return false;
        }
    }

    #endregion
}