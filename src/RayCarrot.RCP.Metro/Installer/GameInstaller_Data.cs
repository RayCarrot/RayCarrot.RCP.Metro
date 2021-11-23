using System.Collections.Generic;
using System.Threading;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Install data for installing a game
/// </summary>
public class GameInstaller_Data
{
    public GameInstaller_Data(List<GameInstaller_Item> installItems, FileSystemPath outputDir, CancellationToken cancellationToken)
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
    public List<GameInstaller_Item> RelativeInputs { get; }
}