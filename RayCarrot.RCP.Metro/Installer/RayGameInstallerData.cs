using System.Collections.Generic;
using System.Threading;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Install data for installing a game
    /// </summary>
    public class RayGameInstallerData
    {
        public RayGameInstallerData(List<RayGameInstallItem> installItems, FileSystemPath outputDir, CancellationToken cancellationToken)
        {
            RelativeInputs = installItems;
            OutputDir = outputDir;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// The <see cref="CancellationToken"/> used to cancel the installation
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// The output directory for the installation
        /// </summary>
        public FileSystemPath OutputDir { get; }
    
        /// <summary>
        /// The install items
        /// </summary>
        public List<RayGameInstallItem> RelativeInputs { get; }
    }
}