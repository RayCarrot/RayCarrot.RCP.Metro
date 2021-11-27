#nullable disable
using System.Collections.ObjectModel;
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a help item
/// </summary>
public class Page_Help_ItemViewModel : BaseViewModel
{
    #region Public Properties

    /// <summary>
    /// The display header
    /// </summary>
    public string DisplayHeader { get; set; }

    /// <summary>
    /// The help text
    /// </summary>
    public string HelpText { get; set; }

    /// <summary>
    /// Cheat code items
    /// </summary>
    public Page_Help_BaseCheatCodeItemViewModel[] CheatCodeItems { get; set; }

    /// <summary>
    /// The cheat code item type
    /// </summary>
    public CheatCodeItemsType CheatCodeType
    {
        get
        {
            if (CheatCodeItems?.Any() != true)
                return CheatCodeItemsType.None;

            var item = CheatCodeItems.First();

            if (item is Page_Help_Rayman1CheatCodeItemViewModel)
                return CheatCodeItemsType.Rayman1;

            if (item is Page_Help_GenericCheatCodeItemViewModel)
                return CheatCodeItemsType.Generic;

            return CheatCodeItemsType.None;
        }
    }

    /// <summary>
    /// True if there are sub items, false if not
    /// </summary>
    public bool HasSubItems => SubItems != null;

    /// <summary>
    /// The sub items
    /// </summary>
    public ObservableCollection<Page_Help_ItemViewModel> SubItems { get; set; }

    /// <summary>
    /// The required <see cref="UserLevel"/>
    /// </summary>
    public UserLevel RequiredUserLevel { get; set; } = UserLevel.Normal;

    #endregion

    #region Enums

    /// <summary>
    /// The available cheat code item types
    /// </summary>
    public enum CheatCodeItemsType
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Rayman 1
        /// </summary>
        Rayman1,

        /// <summary>
        /// Generic item with single input
        /// </summary>
        Generic
    }

    #endregion
}