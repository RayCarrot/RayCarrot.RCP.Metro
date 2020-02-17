using System;
using System.Collections.Generic;
using System.Linq;
using RayCarrot.Extensions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    // TODO: Move to RayCarrot.IO
    /// <summary>
    /// A file extension
    /// </summary>
    public class FileExtension
    {
        #region Constructors

        /// <summary>
        /// Constructor for a complete file extension or file name
        /// </summary>
        /// <param name="fileExtensions">The complete file extension or file name</param>
        public FileExtension(string fileExtensions)
        {
            AllFileExtensions = fileExtensions.ToLower().Split('.').Skip(1).Select(x => $".{x}").ToArray();
        }

        /// <summary>
        /// Constructor for a collection of file extensions
        /// </summary>
        /// <param name="fileExtensions">The file extensions</param>
        public FileExtension(IEnumerable<string> fileExtensions)
        {
            AllFileExtensions = fileExtensions.Select(x => x.ToLower()).ToArray();
        }

        #endregion

        #region Public Properties

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

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the other instance is equals to the current one
        /// </summary>
        /// <param name="other">The other instance to compare to the current one</param>
        /// <returns>True if the other instance is equals to the current one, false if not</returns>
        public bool Equals(FileExtension other)
        {
            return FileExtensions == other.FileExtensions;
        }

        /// <summary>
        /// True if the specified object equals the current instance
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is FileExtension path && Equals(path);
        }

        /// <summary>
        /// Returns the hash code for this instance
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance</returns>
        public override int GetHashCode()
        {
            return FileExtensions.GetHashCode();
        }

        /// <summary>
        /// Gets the <see cref="DisplayName"/> for the extension
        /// </summary>
        /// <returns>The extension display name</returns>
        public override string ToString() => DisplayName;

        #endregion

        #region Static Operators

        /// <summary>
        /// Checks if the two paths are the same
        /// </summary>
        /// <param name="a">The first path</param>
        /// <param name="b">The second path</param>
        /// <returns>True if they are the same, false if not</returns>
        public static bool operator ==(FileExtension a, FileExtension b)
        {
            if (a is null)
                return b is null;

            return a.Equals(b);
        }

        /// <summary>
        /// Checks if the two paths are not the same
        /// </summary>
        /// <param name="a">The first path</param>
        /// <param name="b">The second path</param>
        /// <returns>True if they are not the same, false if they are</returns>
        public static bool operator !=(FileExtension a, FileExtension b)
        {
            return !(a == b);
        }

        #endregion
    }
}