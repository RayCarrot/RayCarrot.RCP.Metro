using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using RayCarrot.Extensions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the educational DOS games options
    /// </summary>
    public class EducationalDosOptionsViewModel : BaseRCPViewModel, IDisposable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EducationalDosOptionsViewModel()
        {
            Games = Data.EducationalDosBoxGames.ToObservableCollection();
        }

        public ObservableCollection<EducationalDosBoxGameInfo> Games { get; }

        public void Dispose()
        {
            MessageBox.Show("");
        }
    }
}