using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A game purchase link which can be accessed from the game
    /// </summary>
    public class GamePurchaseLink
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="header">The link header</param>
        /// <param name="path">The link path</param>
        /// <param name="icon">The icon to use</param>
        public GamePurchaseLink(string header, string path, PackIconMaterialKind icon = PackIconMaterialKind.BriefcaseOutline)
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
        /// The link path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The icon to use
        /// </summary>
        public PackIconMaterialKind Icon { get; }
    }
}