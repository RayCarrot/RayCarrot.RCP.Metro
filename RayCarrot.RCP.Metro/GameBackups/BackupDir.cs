﻿using RayCarrot.IO;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Contains information regarding a directory to include in backup
    /// </summary>
    public class BackupDir : IOSearchPattern
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="dirPath">The directory path</param>
        /// <param name="searchOption">The search option to use when finding files and sub directories</param>
        /// <param name="searchPattern">The search pattern to use when finding files and sub directories</param>
        /// <param name="id">The ID of the <see cref="BackupDir"/></param>
        /// <param name="backupVersion">Indicates which backup version the directory info belongs to</param>
        public BackupDir(FileSystemPath dirPath, SearchOption searchOption, string searchPattern, string id, int backupVersion) : base(dirPath, searchOption, searchPattern)
        {
            ID = id;
            BackupVersion = backupVersion;
        }

        /// <summary>
        /// The ID of the <see cref="BackupDir"/>
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Indicates which backup version the directory info belongs to
        /// </summary>
        public int BackupVersion { get; }
    }
}