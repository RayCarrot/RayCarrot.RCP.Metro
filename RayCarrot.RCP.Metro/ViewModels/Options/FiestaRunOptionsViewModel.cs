namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Fiesta Run options
    /// </summary>
    public class FiestaRunOptionsViewModel : BaseRCPViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FiestaRunOptionsViewModel()
        {
            // Get the manager
            var manager = Games.RaymanFiestaRun.GetManager<RaymanFiestaRun_WinStore>(GameType.WinStore);

            // Get available versions
            IsFiestaRunDefaultAvailable = manager.GetGamePackage(manager.GetFiestaRunPackageName(FiestaRunEdition.Default)) != null;
            IsFiestaRunPreloadAvailable = manager.GetGamePackage(manager.GetFiestaRunPackageName(FiestaRunEdition.Preload)) != null;
            IsFiestaRunWin10Available = manager.GetGamePackage(manager.GetFiestaRunPackageName(FiestaRunEdition.Win10)) != null;
        }

        /// <summary>
        /// Indicates if <see cref="FiestaRunEdition.Default"/> is available
        /// </summary>
        public bool IsFiestaRunDefaultAvailable { get; }

        /// <summary>
        /// Indicates if <see cref="FiestaRunEdition.Preload"/> is available
        /// </summary>
        public bool IsFiestaRunPreloadAvailable { get; }

        /// <summary>
        /// Indicates if <see cref="FiestaRunEdition.Win10"/> is available
        /// </summary>
        public bool IsFiestaRunWin10Available { get; }
    }
}