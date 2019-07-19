using RayCarrot.IO;

namespace RayCarrot.RCP.Metro.Legacy
{
    /// <summary>
    /// Legacy game user data for version 2.x - 3.x
    /// </summary>
    public class LegacyGameUserData
    {
        public FileSystemPath? DosBoxExe { get; set; }

        public FileSystemPath? DosBoxConfig { get; set; }

        public LegacyRaymanGame[] RayGames { get; set; }

        public bool? TPLSIsInstalled { get; set; }

        public FileSystemPath? TPLSDir { get; set; }

        public LegacyTPLSRaymanVersion? TPLSRaymanVersion { get; set; }

        public LegacyTPLSDOSBoxVersion? TPLSDOSBoxVersion { get; set; }
    }
}