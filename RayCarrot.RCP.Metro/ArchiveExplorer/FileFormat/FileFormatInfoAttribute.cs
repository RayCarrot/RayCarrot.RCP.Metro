using System;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Specifies information for a file format, to be used on an enum field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FileFormatInfoAttribute : Attribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileExtension">The file extension</param>
        /// <param name="magicHeader">The magic header</param>
        public FileFormatInfoAttribute(string fileExtension, uint magicHeader)
        {
            FileExtension = new FileExtension(fileExtension);
            MagicHeader = magicHeader;
        }

        /// <summary>
        /// The file extension
        /// </summary>
        public FileExtension FileExtension { get; }

        /// <summary>
        /// The magic header
        /// </summary>
        public uint MagicHeader { get; }
    }
}