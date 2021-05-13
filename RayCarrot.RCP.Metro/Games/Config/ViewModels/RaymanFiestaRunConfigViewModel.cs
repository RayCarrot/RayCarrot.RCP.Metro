namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Fiesta Run configuration
    /// </summary>
    public class RaymanFiestaRunConfigViewModel : BaseRayRunConfigViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanFiestaRunConfigViewModel() : base(Games.RaymanFiestaRun) 
        { }

        public override bool ReloadOnGameInfoChanged => true;

        protected override object GetPageUI() => new RaymanFiestaRunConfig()
        {
            DataContext = this
        };
    }
}