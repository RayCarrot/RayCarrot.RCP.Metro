#nullable disable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

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
        foreach (GameInstallation gameInstallation in Services.App.GetInstalledGames)
        {
            foreach (var item in gameInstallation.Game.GetManager().GetJumpListItems())
            {
                if (item.IsIncluded)
                    included.Add(item);
                else
                    NotIncluded.Add(item);
            }
        }

        // Order the included games
        Included = included.OrderBy(x => Services.Data.App_JumpListItemIDCollection.IndexOf(x.ID)).ToObservableCollection();
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

    #region Public Methods

    public async Task LoadIconsAsync()
    {
        await Task.Run(() =>
        {
            foreach (JumpListItemViewModel item in Included.Concat(NotIncluded).ToArray())
                item.SetIconImageSource();
        });
    }

    #endregion
}