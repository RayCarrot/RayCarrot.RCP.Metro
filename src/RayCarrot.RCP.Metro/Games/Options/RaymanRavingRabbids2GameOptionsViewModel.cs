namespace RayCarrot.RCP.Metro.Games.Options;

/// <summary>
/// View model for the Rayman Raving Rabbids 2 game options
/// </summary>
public class RaymanRavingRabbids2GameOptionsViewModel : GameOptionsViewModel
{
    #region Constructor

    public RaymanRavingRabbids2GameOptionsViewModel(GameInstallation gameInstallation) : base(gameInstallation) { }

    #endregion

    #region Public Properties

    public UserData_RRR2LaunchMode LaunchMode
    {
        get => GameInstallation.GetValue(GameDataKey.RRR2_LaunchMode, UserData_RRR2LaunchMode.AllGames);
        set
        {
            GameInstallation.SetValue(GameDataKey.RRR2_LaunchMode, value);
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }

    #endregion
}