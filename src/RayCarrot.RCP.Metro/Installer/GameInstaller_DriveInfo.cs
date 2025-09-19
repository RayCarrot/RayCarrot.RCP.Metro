﻿#nullable disable
using System.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Drive information for a game installation
/// </summary>
public class GameInstaller_DriveInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="root">The root directory</param>
    /// <param name="volumeLabel">The volume label</param>
    public GameInstaller_DriveInfo(FileSystemPath root, string volumeLabel)
    {
        Root = root;
        VolumeLabel = volumeLabel;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The root directory
    /// </summary>
    public FileSystemPath Root { get; }

    /// <summary>
    /// The volume label
    /// </summary>
    public string VolumeLabel { get; }

    /// <summary>
    /// True if the drive is available, false if not
    /// </summary>
    public bool IsAvailable
    {
        get
        {
            try
            {
                return Root.DirectoryExists && new DriveInfo(Root).VolumeLabel == VolumeLabel;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Checking if saved drive is available");
                return false;
            }
        }
    }
}