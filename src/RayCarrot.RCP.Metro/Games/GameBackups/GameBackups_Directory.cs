using System;
using System.Collections.Generic;
using RayCarrot.IO;
using System.IO;
using System.Linq;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Contains information regarding a directory to include in backup
/// </summary>
public class GameBackups_Directory
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="dirPath">The directory path</param>
    /// <param name="searchOption">The search option to use when finding files and sub directories</param>
    /// <param name="searchPattern">The search pattern to use when finding files and sub directories</param>
    /// <param name="id">The ID of the <see cref="GameBackups_Directory"/></param>
    /// <param name="backupVersion">Indicates which backup version the directory info belongs to</param>
    public GameBackups_Directory(FileSystemPath dirPath, SearchOption searchOption, string? searchPattern, string id, int backupVersion)
    {
        DirPath = dirPath;
        SearchOption = searchOption;
        SearchPattern = searchPattern ?? "*";
        ID = id;
        BackupVersion = backupVersion;
        VirtualStoreDirPath = GetVirtualStoreFilePath() + DirPath.RemoveRoot();
        HasVirtualStoreVersion = VirtualStoreDirPath.DirectoryExists;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    /// <summary>
    /// The directory path
    /// </summary>
    private FileSystemPath DirPath { get; }

    private FileSystemPath VirtualStoreDirPath { get; }

    /// <summary>
    /// The search option to use when finding files and sub directories
    /// </summary>
    private SearchOption SearchOption { get; }

    /// <summary>
    /// The search pattern to use when finding files and sub directories
    /// </summary>
    private string SearchPattern { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The ID of the <see cref="GameBackups_Directory"/>
    /// </summary>
    public string ID { get; }

    /// <summary>
    /// Indicates which backup version the directory info belongs to
    /// </summary>
    public int BackupVersion { get; }

    /// <summary>
    /// Indicates if there is a VirtualStore version of this directory
    /// </summary>
    public bool HasVirtualStoreVersion { get; }

    #endregion

    #region Private Methods

    private static FileSystemPath GetVirtualStoreFilePath() => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "VirtualStore";

    private static DateTime GetLastWriteTime(IOSearchPattern dir) => dir.GetFiles().
        Select(x => new FileInfo(x).LastWriteTime).
        OrderByDescending(x => x).
        FirstOrDefault();

    private FileSystemPath GetBackupDirPath(ProgramDataSource source)
    {
        Logger.Trace("Getting directory for {0} with source {1}", DirPath, source);

        if (source == ProgramDataSource.Default)
        {
            Logger.Trace("Determined to use default directory based on source value");
            return DirPath;
        }

        if (source == ProgramDataSource.VirtualStore)
        {
            Logger.Trace("Determined to use VirtualStore directory based on source value");
            return VirtualStoreDirPath;
        }

        if (!VirtualStoreDirPath.DirectoryExists)
        {
            Logger.Trace("Determined to use default directory due to the VirtualStore directory not existing");
            return DirPath;
        }

        if (!DirPath.DirectoryExists)
        {
            Logger.Trace("Determined to use VirtualStore directory due to the default directory not existing");
            return VirtualStoreDirPath;
        }

        try
        {
            DateTime originalTime = GetLastWriteTime(new IOSearchPattern(DirPath, SearchOption, SearchPattern));
            DateTime virtualStoreTime = GetLastWriteTime(new IOSearchPattern(VirtualStoreDirPath, SearchOption, SearchPattern));

            if (originalTime > virtualStoreTime)
            {
                Logger.Trace("Determined to use default directory based on last write time");
                return DirPath;
            }
            else
            {
                Logger.Trace("Determined to use VirtualStore directory based on last write time");
                return VirtualStoreDirPath;
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Determining directory to use based on last write time");

            // Use the VirtualStore path in case of error since this is most likely to work then
            return VirtualStoreDirPath;
        }
    }

    #endregion

    #region Public Methods

    public IEnumerable<BackupSearchPattern> GetBackupSearchPatterns(ProgramDataSource source, OperationType operation)
    {
        if (source == ProgramDataSource.Auto && operation == OperationType.Write)
        {
            // Return both paths if available. If the source if auto then we want to write to both locations.
            yield return new BackupSearchPattern(ID, new IOSearchPattern(DirPath, SearchOption, SearchPattern));

            if (HasVirtualStoreVersion)
                yield return new BackupSearchPattern(ID, new IOSearchPattern(VirtualStoreDirPath, SearchOption, SearchPattern));
        }
        else
        {
            // Find and return a single matching directory path to use
            yield return new BackupSearchPattern(ID, new IOSearchPattern(GetBackupDirPath(source), SearchOption, SearchPattern));
        }
    }

    #endregion

    #region Enums

    public enum OperationType
    {
        /// <summary>
        /// Reading from the game progress (backup)
        /// </summary>
        Read,

        /// <summary>
        /// Writing to the game progress (restore)
        /// </summary>
        Write,
    }

    #endregion
}

public record BackupSearchPattern(string ID, IOSearchPattern SearchPattern);