using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the educational DOSBox game configuration
    /// </summary>
    public class EducationalDosBoxGameConfigViewModel : BaseDosBoxConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Constructor for using the default game, <see cref="Games.EducationalDos"/>
        /// </summary>
        public EducationalDosBoxGameConfigViewModel() : base(Games.EducationalDos)
        {
            IsMountLocationAvailable = false;
            IsGameLanguageAvailable = false;
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Performs additional setup for the game
        /// </summary>
        /// <returns>The task</returns>
        protected override Task SetupGameAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs additional saving for the game
        /// </summary>
        /// <returns>The task</returns>
        protected override Task SaveGameAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}