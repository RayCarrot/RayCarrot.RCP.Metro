using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RayCarrot.Common;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for editing the jump list
    /// </summary>
    public class JumpListEditViewModel : UserInputViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public JumpListEditViewModel()
        {
            // Set the title
            Title = Resources.JumpListEditor_Header;

            // Create the collections
            var included = new List<JumpListItemViewModel>();
            NotIncluded = new ObservableCollection<JumpListItemViewModel>();

            // Get all jump list items
            foreach (var game in RCPServices.App.GetGames.Where(x => x.IsAdded()))
            {
                foreach (var item in game.GetManager().GetJumpListItems())
                {
                    if (item.IsIncluded)
                        included.Add(item);
                    else
                        NotIncluded.Add(item);

                    item.SetIconImageSource();
                }
            }

            // Order the included games
            Included = included.OrderBy(x => RCPServices.Data.JumpListItemIDCollection.IndexOf(x.ID)).ToObservableCollection();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The included items
        /// </summary>
        public ObservableCollection<JumpListItemViewModel> Included { get; }

        /// <summary>
        /// The not included items
        /// </summary>
        public ObservableCollection<JumpListItemViewModel> NotIncluded { get; }

        #endregion
    }
}