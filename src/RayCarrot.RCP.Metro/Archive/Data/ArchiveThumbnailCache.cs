using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Thumbnail cache for an archive
/// </summary>
public class ArchiveThumbnailCache : IDisposable
{
    #region Constants

    // IDEA: Allow these values to be changed in settings?
    private const int CacheMaxCount = 250;
    private const int CacheClearCycleCount = 15;

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private object Lock { get; } = new();
    private Dictionary<string, CacheEntry> Cache { get; } = new();
    private List<string> CacheRegister { get; } = new();

    #endregion

    #region Protected Methods

    protected void AddEntry(string key, CacheEntry entry)
    {
        lock (Lock)
        {
            Cache[key] = entry;

            if (CacheRegister.Contains(key))
                CacheRegister.Remove(key);

            CacheRegister.Add(key);
        }
    }

    protected void RemoveEntry(string key)
    {
        lock (Lock)
        {
            Cache.Remove(key);
            CacheRegister.Remove(key);
        }
    }

    protected void ClearOldEntries(int count)
    {
        lock (Lock)
        {
            int clearCount = count.Clamp(0, Cache.Count);

            for (int i = 0; i < clearCount; i++)
            {
                Cache.Remove(CacheRegister[0]);
                CacheRegister.RemoveAt(0);
            }

            Logger.Info("Cleared {0} entries from thumbnail cache", clearCount);
        }
    }

    #endregion

    #region Public Methods

    public void AddToCache(ArchiveFileViewModel file, ArchiveFileThumbnailData thumb)
    {
        if (Cache.Count >= CacheMaxCount)
            ClearOldEntries(CacheClearCycleCount);

        AddEntry(file.FullFilePath, new CacheEntry(new WeakReference<ArchiveFileViewModel>(file), thumb));
    }

    public bool TryGetCachedItem(ArchiveFileViewModel file, [MaybeNullWhen(false)] out ArchiveFileThumbnailData thumb)
    {
        // Default to null
        thumb = null;

        // Get the path
        string path = file.FullFilePath;

        // Check if the path exists in the cache
        if (!Cache.ContainsKey(path))
            return false;

        // Get the cached entry
        CacheEntry entry = Cache[path];

        // Check if the file instance matches
        if (entry.File.TryGetTarget(out ArchiveFileViewModel f) && f == file)
        {
            // Set the cached data
            thumb = entry.Thumb;
            return true;
        }
        else
        {
            // Remove the cached entry
            RemoveEntry(path);
            return false;
        }
    }

    public void Dispose()
    {
        Cache.Clear();
        CacheRegister.Clear();
    }

    #endregion

    #region Protected Records

    protected record CacheEntry(WeakReference<ArchiveFileViewModel> File, ArchiveFileThumbnailData Thumb);

    #endregion
}