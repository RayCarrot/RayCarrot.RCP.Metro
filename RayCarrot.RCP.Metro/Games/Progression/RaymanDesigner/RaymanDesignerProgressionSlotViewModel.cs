using RayCarrot.IO;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a Rayman Designer progression slot item
    /// </summary>
    public class RaymanDesignerProgressionSlotViewModel : ProgressionSlotViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="slotName">The slot name</param>
        /// <param name="items">The progression info items</param>
        /// <param name="progressionViewModel">The progression view model containing this slot</param>
        public RaymanDesignerProgressionSlotViewModel(LocalizedString slotName, ProgressionInfoItemViewModel[] items, BaseProgressionViewModel progressionViewModel) : base(slotName, items, FileSystemPath.EmptyPath, progressionViewModel)
        {

        }

        /// <summary>
        /// Indicates if the slot can be exported/imported
        /// </summary>
        public override bool CanModify => false;

        /// <summary>
        /// Exports the save slot from the specified path
        /// </summary>
        /// <param name="outputFilePath">The output file path</param>
        /// <returns>The task</returns>
        protected override Task ExportSaveDataAsync(FileSystemPath outputFilePath) => Task.CompletedTask;

        /// <summary>
        /// Imports an exported save slot to the save slot from the specified path
        /// </summary>
        /// <param name="inputFilePath">The input file path</param>
        /// <returns>The task</returns>
        protected override Task ImportSaveDataAsync(FileSystemPath inputFilePath) => Task.CompletedTask;
    }
}