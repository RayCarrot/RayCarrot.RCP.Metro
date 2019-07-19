using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro.Legacy
{
    /// <summary>
    /// Legacy app user data for version 2.x - 3.x
    /// </summary>
    public class LegacyAppUserData
    {
        #region Public Properties

        public FileSystemPath? BackupLocation { get; set; }

        public UserLevel? UserLevel { get; set; }

        public ExceptionLevel? DisplayExceptionLevel { get; set; }

        public bool? AutoClose { get; set; }

        public bool? AutoGameCheck { get; set; }

        public bool? AutoCloseConfig { get; set; }

        public bool? ShowActionComplete { get; set; }

        public bool? AutoUpdateCheck { get; set; }

        public bool? ShowTaskBarProgress { get; set; }

        public int? LastVersion { get; set; }

        #endregion
    }
}