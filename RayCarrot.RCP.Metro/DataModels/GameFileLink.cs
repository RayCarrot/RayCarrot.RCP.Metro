using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A game file link which can be accessed from the game
    /// </summary>
    public class GameFileLink
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="header">The link header</param>
        /// <param name="path">The link file path</param>
        /// <param name="icon">The icon to use, or none to use the file icon</param>
        public GameFileLink(string header, FileSystemPath path, PackIconMaterialKind icon = PackIconMaterialKind.None)
        {
            Header = header;
            Path = path;
            Icon = icon;
        }

        /// <summary>
        /// The link header
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// The link file path
        /// </summary>
        public FileSystemPath Path { get; }

        /// <summary>
        /// The icon to use, or none to use the file icon
        /// </summary>
        public PackIconMaterialKind Icon { get; }
    }
}