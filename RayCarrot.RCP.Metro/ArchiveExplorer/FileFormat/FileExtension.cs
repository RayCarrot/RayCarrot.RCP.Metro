using System;
using System.Collections.Generic;
using System.Linq;
using RayCarrot.Extensions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A file extension
    /// </summary>
    public class FileExtension
    {
        /// <summary>
        /// Constructor for a complete file extension or file name
        /// </summary>
        /// <param name="fileExtensions">The complete file extension or file name</param>
        public FileExtension(string fileExtensions)
        {
            AllFileExtensions = fileExtensions.Split('.').Skip(1).Select(x => $".{x}").ToArray();
        }

        /// <summary>
        /// Constructor for a collection of file extensions
        /// </summary>
        /// <param name="fileExtensions">The file extensions</param>
        public FileExtension(IEnumerable<string> fileExtensions)
        {
            AllFileExtensions = fileExtensions.ToArray();
        }

        /// <summary>
        /// The available file extensions, in order
        /// </summary>
        protected string[] AllFileExtensions { get; }

        /// <summary>
        /// All file extensions, combined into one
        /// </summary>
        public string FileExtensions => AllFileExtensions.JoinItems(String.Empty);

        /// <summary>
        /// The primary file extension
        /// </summary>
        public string PrimaryFileExtension => AllFileExtensions.Last();

        /// <summary>
        /// The display name for the extension
        /// </summary>
        public string DisplayName => FileExtensions.ToUpperInvariant();

        /// <summary>
        /// Gets a file filter item for the file extension. This only includes the primary one.
        /// </summary>
        public FileFilterItem GetFileFilterItem => new FileFilterItem($"*{PrimaryFileExtension}", PrimaryFileExtension.Substring(1).ToUpperInvariant());

        /// <summary>
        /// Gets the <see cref="DisplayName"/> for the extension
        /// </summary>
        /// <returns>The extension display name</returns>
        public override string ToString() => DisplayName;
    }
}