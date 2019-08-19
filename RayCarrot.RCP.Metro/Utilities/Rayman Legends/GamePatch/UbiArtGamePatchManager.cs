using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Used to manage game patches for a UbiArt game
    /// </summary>
    public class UbiArtGamePatchManager
    {
        public UbiArtGamePatchManager(FileSystemPath engineDirectory)
        {
            EngineDirectory = engineDirectory;
        }

        public FileSystemPath EngineDirectory { get; }

        public async Task LoadPatchesAsync()
        {

        }
    }
}