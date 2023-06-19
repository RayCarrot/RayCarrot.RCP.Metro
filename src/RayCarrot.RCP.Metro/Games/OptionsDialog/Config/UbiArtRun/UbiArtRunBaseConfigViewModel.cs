using System.IO;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

// TODO: Rewrite this to use BinarySerializer instead?

/// <summary>
/// Base for Rayman Jungle/Fiesta Run config view model
/// </summary>
public abstract class UbiArtRunBaseConfigViewModel : ConfigPageViewModel
{
    #region Constructor

    protected UbiArtRunBaseConfigViewModel(
        GameDescriptor gameDescriptor, 
        GameInstallation gameInstallation,
        FileSystemPath saveDir,
        bool isUpc)
    {
        // Set properties
        GameDescriptor = gameDescriptor;
        GameInstallation = gameInstallation;
        SaveDir = saveDir;
        IsUpc = isUpc;

        UpcStorageHeaders = new Dictionary<string, byte[]>();
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

    private GameInstallation GameInstallation { get; }
    private GameDescriptor GameDescriptor { get; }
    private FileSystemPath SaveDir { get; }

    #endregion

    #region Protected Properties

    protected Dictionary<string, byte[]> UpcStorageHeaders { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the game uses UPC (Ubisoft/Uplay PC?) storage files. This is used in the Ubisoft Connect releases.
    /// </summary>
    public bool IsUpc { get; }

    public bool IsVolumeSettingsAvailable => !IsUpc || UpcStorageHeaders.ContainsKey(SelectedVolumeFileName);

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

    protected FileSystemPath GetFilePath(string fileName)
    {
        // UPC files always use the .save file extension
        if (IsUpc)
            return SaveDir + $"{fileName}.save";
        else    
            return SaveDir + fileName;
    }

    /// <summary>
    /// Reads a single byte from the specified file relative to the current save data
    /// </summary>
    /// <param name="fileName">The file name, relative to the current save data</param>
    /// <returns>The byte or null if not found</returns>
    protected virtual byte? ReadSingleByteFile(string fileName)
    {
        return ReadMultiByteFile(fileName, 1)?.FirstOrDefault();
    }

    /// <summary>
    /// Reads multiple bytes from the specified file relative to the current save data
    /// </summary>
    /// <param name="fileName">The file name, relative to the current save data</param>
    /// <param name="length">The amount of bytes to read</param>
    /// <returns>The bytes or null if not found</returns>
    protected virtual byte[]? ReadMultiByteFile(string fileName, int length)
    {
        // Get the file path
        FileSystemPath filePath = GetFilePath(fileName);

        // Make sure the file exists
        if (!filePath.FileExists)
            return null;

        // Create the file stream
        using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        // Use a reader
        using Reader reader = new(stream, isLittleEndian: true);

        // Read and save UPC header
        if (IsUpc)
        {
            int headerSize = reader.ReadInt32();
            byte[] header = reader.ReadBytes(headerSize);
            UpcStorageHeaders[fileName] = header;
        }

        // Read and return the bytes
        return reader.ReadBytes(length);
    }

    /// <summary>
    /// Writes a single byte to the specified file relative to the current save data
    /// </summary>
    /// <param name="fileName">The file name, relative to the current save data</param>
    /// <param name="value">The byte to write</param>
    protected virtual void WriteSingleByteFile(string fileName, byte value)
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
    protected virtual void WriteMultiByteFile(string fileName, byte[] value)
    {
        // Get the file path
        FileSystemPath filePath = GetFilePath(fileName);

        // Create the file stream
        using FileStream stream = new(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

        // Use a writer
        using Writer writer = new(stream, isLittleEndian: true);

        // Write the UPC header
        if (IsUpc)
        {
            byte[] header = UpcStorageHeaders[fileName];
            writer.Write((int)header.Length);
            writer.Write(header);
        }

        // Write the bytes
        writer.Write(value);
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

        AddConfigLocation(LinkItemViewModel.LinkType.BinaryFile, GetFilePath(SelectedVolumeFileName));

        // Read game specific values
        await SetupGameAsync();

        // Read volume
        byte[]? ROvolume = ReadMultiByteFile(SelectedVolumeFileName, 2);
        OnPropertyChanged(nameof(IsVolumeSettingsAvailable));
        MusicVolume = ROvolume?[0] ?? 99;
        SoundVolume = ROvolume?[1] ?? 99;

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
            if (IsVolumeSettingsAvailable)
            {
                WriteMultiByteFile(SelectedVolumeFileName, new byte[]
                {
                    MusicVolume,
                    SoundVolume
                });
            }

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