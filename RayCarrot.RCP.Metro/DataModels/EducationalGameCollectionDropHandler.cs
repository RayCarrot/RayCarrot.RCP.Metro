using GongSolutions.Wpf.DragDrop;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The drop handler for <see cref="EducationalDosOptions"/> games collection
    /// </summary>
    public class EducationalGameCollectionDropHandler : DefaultDropHandler
    {
        /// <summary>
        /// The options view model
        /// </summary>
        public EducationalDosOptionsViewModel ViewModel { get; set; }

        public override async void Drop(IDropInfo dropInfo)
        {
            // Call base drop handler
            base.Drop(dropInfo);
            
            RL.Logger?.LogDebugSource("The educational games have been reordered");

            // Save the new order
            await ViewModel.SaveAsync();
        }
    }
}