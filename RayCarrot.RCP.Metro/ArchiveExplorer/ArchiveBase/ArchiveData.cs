using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Generic archive data
    /// </summary>
    public class ArchiveData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="directories">The directories and their data</param>
        /// <param name="generator">The file generator</param>
        public ArchiveData(IEnumerable<ArchiveDirectory> directories, IDisposable generator)
        {
            Directories = directories;
            Generator = generator;
        }

        /// <summary>
        /// The directories and their data
        /// </summary>
        public IEnumerable<ArchiveDirectory> Directories { get; }

        /// <summary>
        /// The file generator
        /// </summary>
        public IDisposable Generator { get; }
    }
}