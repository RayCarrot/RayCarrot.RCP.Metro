using GongSolutions.Wpf.DragDrop;
using RayCarrot.CarrotFramework.Abstractions;

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
            base.Drop(dropInfo);
            
            RCFCore.Logger?.LogDebugSource("The educational games have been reordered");

            await ViewModel.SaveAsync();
        }
    }
}