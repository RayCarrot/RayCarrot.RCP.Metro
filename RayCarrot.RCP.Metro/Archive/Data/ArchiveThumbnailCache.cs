using System;
using System.Collections.Generic;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
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
                var clearCount = count.Clamp(0, Cache.Count);

                for (int i = 0; i < clearCount; i++)
                {
                    Cache.Remove(CacheRegister[0]);
                    CacheRegister.RemoveAt(0);
                }

                RL.Logger?.LogInformationSource($"Cleared {clearCount} entries from thumbnail cache");
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

        public bool TryGetCachedItem(ArchiveFileViewModel file, out ArchiveFileThumbnailData thumb)
        {
            // Default to null
            thumb = null;

            // Get the path
            var path = file.FullFilePath;

            // Check if the path exists in the cache
            if (!Cache.ContainsKey(path))
                return false;

            // Get the cached entry
            var entry = Cache[path];

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
}