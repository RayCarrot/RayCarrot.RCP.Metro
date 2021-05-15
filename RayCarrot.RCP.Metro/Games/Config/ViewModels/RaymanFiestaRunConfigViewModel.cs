using System.Threading.Tasks;

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

        protected override Task OnGameInfoModified() => LoadPageAsync();

        protected override object GetPageUI() => new RaymanFiestaRunConfig()
        {
            DataContext = this
        };
    }
}