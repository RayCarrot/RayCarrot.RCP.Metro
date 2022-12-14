using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

public class GameDebugViewModel : BaseViewModel, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>
{
    public GameDebugViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
        IconSizes = new ObservableCollection<GameIcon.GameIconSize>(EnumHelpers.GetValues<GameIcon.GameIconSize>());
        RefreshGameInstallations();

        Services.Messenger.RegisterAll(this);
    }

    private GameInstallation _gameInstallation;

    public ObservableCollection<GameInstallation>? GameInstallations { get; set; }

    public GameInstallation GameInstallation
    {
        get => _gameInstallation;
        [MemberNotNull(nameof(_gameInstallation))]
        [MemberNotNull(nameof(GameDescriptor))]
        [MemberNotNull(nameof(DisplayName))]
        [MemberNotNull(nameof(GameInstallationJToken))]
        set
        {
            _gameInstallation = value;
            Refresh();
        }
    }

    public GameDescriptor GameDescriptor { get; private set; }
    public bool IsDemo { get; private set; }
    public GameIconAsset Icon { get; private set; }
    public string DisplayName { get; private set; }
    public JToken GameInstallationJToken { get; private set; }

    public ObservableCollection<GameIcon.GameIconSize> IconSizes { get; }

    [MemberNotNull(nameof(GameDescriptor))]
    [MemberNotNull(nameof(DisplayName))]
    [MemberNotNull(nameof(GameInstallationJToken))]
    private void Refresh()
    {
        GameDescriptor = GameInstallation.GameDescriptor;
        IsDemo = GameDescriptor.IsDemo;
        Icon = GameDescriptor.Icon;
        DisplayName = GameDescriptor.DisplayName;
        GameInstallationJToken = JToken.FromObject(GameInstallation);
    }

    public void RefreshGameInstallations()
    {
        GameInstallations = new ObservableCollection<GameInstallation>(Services.Games.GetInstalledGames());
        Refresh();
    }

    public void Receive(AddedGamesMessage message) => RefreshGameInstallations();
    public void Receive(RemovedGamesMessage message) => RefreshGameInstallations();
    public void Receive(ModifiedGamesMessage message)
    {
        if (message.GameInstallations.Contains(GameInstallation))
            Refresh();
    }
}