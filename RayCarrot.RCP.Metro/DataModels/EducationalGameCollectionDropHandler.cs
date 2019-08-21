using GongSolutions.Wpf.DragDrop;

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

            // TODO: Log

            await ViewModel.SaveAsync();
        }
    }
}