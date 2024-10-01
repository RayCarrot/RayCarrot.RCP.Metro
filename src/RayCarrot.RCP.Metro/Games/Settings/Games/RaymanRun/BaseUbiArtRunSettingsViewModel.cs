using System.IO;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Settings;

// TODO: Rewrite this to use BinarySerializer instead?

/// <summary>
/// Base for Rayman Jungle/Fiesta Run settings view model
/// </summary>
public abstract class BaseUbiArtRunSettingsViewModel : GameSettingsViewModel
{
    #region Constructor

    protected BaseUbiArtRunSettingsViewModel(GameInstallation gameInstallation, FileSystemPath saveDir, bool isUpc) : base(gameInstallation)
    {
        // Set properties
        SaveDir = saveDir;
        IsUpc = isUpc;

        UpcStorageHeaders = new Dictionary<string, byte[]>();
    }

    #endregion

    #region Private Constants

    private const string SelectedVolumeFileName = "ROvolume";

    #endregion

    #region Private Properties

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
    public byte MusicVolume { get; set; }

    /// <summary>
    /// The sound volume, a value between 0 and 99
    /// </summary>
    public byte SoundVolume { get; set; }

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
        WriteMultiByteFile(fileName, new[] { value });
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

    protected override async Task LoadAsync()
    {
        AddSettingsLocation(LinkItemViewModel.LinkType.BinaryFile, GetFilePath(SelectedVolumeFileName));

        // Read game specific values
        await SetupGameAsync();

        // Read volume
        byte[]? ROvolume = ReadMultiByteFile(SelectedVolumeFileName, 2);
        OnPropertyChanged(nameof(IsVolumeSettingsAvailable));
        MusicVolume = ROvolume?[0] ?? 99;
        SoundVolume = ROvolume?[1] ?? 99;

        UnsavedChanges = false;
    }

    protected override async Task SaveAsync()
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
    }

    protected override void SettingsPropertyChanged(string propertyName)
    {
        if (propertyName is
            nameof(MusicVolume) or
            nameof(SoundVolume))
        {
            UnsavedChanges = true;
        }
    }

    #endregion
}