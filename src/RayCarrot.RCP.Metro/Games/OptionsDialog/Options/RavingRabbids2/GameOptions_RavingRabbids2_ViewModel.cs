namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Raving Rabbids 2 options
/// </summary>
public class GameOptions_RavingRabbids2_ViewModel : BaseRCPViewModel
{
    #region Constructor

    public GameOptions_RavingRabbids2_ViewModel(GameInstallation gameInstallation)
    {
        // Set properties
        GameInstallation = gameInstallation;
    }

    #endregion

    #region Private Properties

    public GameInstallation GameInstallation { get; }

    #endregion

    #region Public Properties

    public UserData_RRR2LaunchMode LaunchMode
    {
        get => GameInstallation.GetValue(GameDataKey.RRR2LaunchMode, UserData_RRR2LaunchMode.AllGames);
        set
        {
            GameInstallation.SetValue(GameDataKey.RRR2LaunchMode, value);
            
            // TODO-14: Clean up
            Invoke();
            async void Invoke() => await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(GameInstallation, RefreshFlags.GameInfo | RefreshFlags.LaunchInfo));
        }
    }

    #endregion
}