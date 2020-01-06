namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Fiesta Run configuration
    /// </summary>
    public class RaymanFiestaRunConfigViewModel : BaseRayRunConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanFiestaRunConfigViewModel() : base(Games.RaymanFiestaRun)
        {
            // Reload when the game edition changes
            ReloadOnGameInfoChanged = true;
        }

        #endregion
    }
}