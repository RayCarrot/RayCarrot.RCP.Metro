using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Shortcuts for the Carrot Framework
    /// </summary>
    public static class RCFRCPA
    {
        /// <summary>
        /// The API controller manager
        /// </summary>
        public static IAPIControllerManager API => RCF.GetService<IAPIControllerManager>();

        /// <summary>
        /// The update manager
        /// </summary>
        public static IUpdaterManager UpdaterManager => RCF.GetService<IUpdaterManager>();

        /// <summary>
        /// The file manager
        /// </summary>
        public static IFileManager File => RCF.GetService<IFileManager>();
    }
}