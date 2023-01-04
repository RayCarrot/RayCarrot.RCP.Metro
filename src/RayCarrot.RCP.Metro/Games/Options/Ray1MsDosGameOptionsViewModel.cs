using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Options;

/// <summary>
/// View model for Ray1 MS-DOS game options
/// </summary>
public class Ray1MsDosGameOptionsViewModel : GameOptionsViewModel
{
    #region Constructor

    public Ray1MsDosGameOptionsViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        Ray1MsDosData data = GameInstallation.GetRequiredObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData);
        AvailableVersions = new ObservableCollection<Ray1MsDosData.Version>(data.AvailableVersions);
    }

    #endregion

    #region Public Properties

    public ObservableCollection<Ray1MsDosData.Version> AvailableVersions { get; }

    public Ray1MsDosData.Version SelectedVersion
    {
        get
        {
            string id = GameInstallation.GetRequiredObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData).SelectedVersion;
            return AvailableVersions.First(x => x.Id == id); 
        }
        set
        {
            Ray1MsDosData data = GameInstallation.GetRequiredObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData);
            data.SelectedVersion = value.Id;
            GameInstallation.SetObject(GameDataKey.Ray1_MsDosData, data);

            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }

    #endregion
}