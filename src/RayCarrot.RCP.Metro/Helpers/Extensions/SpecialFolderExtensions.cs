using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="Environment.SpecialFolder"/>
    /// </summary>
    public static class SpecialFolderExtensions
    {
        /// <summary>
        /// Gets the path to the system special folder that is identified by the specified enumeration, and uses a specified option for accessing special folders
        /// </summary>
        /// <param name="folder">An enumerated constant that identifies a system special folder</param>
        /// <param name="option">Specifies options to use for accessing a special folder</param>
        /// <returns>The path to the specified system special folder, if that folder physically exists on your computer; otherwise, an empty string ("").A folder will not physically exist if the operating system did not create it, the existing folder was deleted, or the folder is a virtual directory, such as My Computer, which does not correspond to a physical path.</returns>
        public static FileSystemPath GetFolderPath(this Environment.SpecialFolder folder, Environment.SpecialFolderOption option = Environment.SpecialFolderOption.None)
        {
            return Environment.GetFolderPath(folder, option);
        }
    }
}