using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Options;

/// <summary>
/// View model for the Rayman Edutainment game options
/// </summary>
public class RaymanEdutainmentGameOptionsViewModel : GameOptionsViewModel
{
    #region Constructor

    public RaymanEdutainmentGameOptionsViewModel(GameInstallation gameInstallation) : base(gameInstallation) { }

    #endregion

    #region Public Properties

    public ObservableCollection<string> GameModes => 
        new(GameInstallation.GetRequiredObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData).AvailableGameModes);

    public string GameMode
    {
        get => GameInstallation.GetRequiredObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData).SelectedGameMode;
        set
        {
            Ray1MsDosData data = GameInstallation.GetRequiredObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData);
            data.SelectedGameMode = value;
            GameInstallation.SetObject(GameDataKey.Ray1_MsDosData, data);

            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }

    #endregion
}