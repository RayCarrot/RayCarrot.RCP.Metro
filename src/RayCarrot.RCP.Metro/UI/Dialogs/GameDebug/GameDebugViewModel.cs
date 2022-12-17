using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

public class GameDebugViewModel : BaseViewModel, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>
{
    public GameDebugViewModel(GameInstallation gameInstallation)
    {
        IconSizes = new ObservableCollection<GameIcon.GameIconSize>(EnumHelpers.GetValues<GameIcon.GameIconSize>());
        RefreshGameInstallations(gameInstallation);

        Services.Messenger.RegisterAll(this);
    }

    private InstalledGameViewModel? _selectedGameInstallation;

    public ObservableCollection<InstalledGameViewModel>? GameInstallations { get; set; }

    public InstalledGameViewModel? SelectedGameInstallation
    {
        get => _selectedGameInstallation;
        set
        {
            _selectedGameInstallation = value;
            Refresh();
        }
    }

    public GameDescriptor? GameDescriptor { get; private set; }
    public bool IsDemo { get; private set; }
    public GameIconAsset Icon { get; private set; }
    public JToken? GameInstallationJToken { get; private set; }

    public ObservableCollection<GameIcon.GameIconSize> IconSizes { get; }

    private void Refresh()
    {
        GameDescriptor = SelectedGameInstallation?.GameDescriptor;
        IsDemo = GameDescriptor?.IsDemo ?? false;
        Icon = GameDescriptor?.Icon ?? GameIconAsset.Rayman1;
        GameInstallationJToken = SelectedGameInstallation == null 
            ? null 
            : JToken.FromObject(SelectedGameInstallation.GameInstallation, JsonSerializer.Create(new JsonSerializerSettings() 
            {
                Converters = new JsonConverter[] { new StringEnumConverter() }
            }));
    }

    public void RefreshGameInstallations(GameInstallation? selectedGameInstallation)
    {
        GameInstallations = new ObservableCollection<InstalledGameViewModel>(Services.Games.GetInstalledGames().
            Select(x => new InstalledGameViewModel(x, x.GameDescriptor, x.GetDisplayName())));

        SelectedGameInstallation = GameInstallations.FirstOrDefault(x => x.GameInstallation == selectedGameInstallation) ?? 
                                   GameInstallations.FirstOrDefault();

        Refresh();
    }

    public void Receive(AddedGamesMessage message) => RefreshGameInstallations(SelectedGameInstallation?.GameInstallation);
    public void Receive(RemovedGamesMessage message) => RefreshGameInstallations(SelectedGameInstallation?.GameInstallation);
    public void Receive(ModifiedGamesMessage message) => RefreshGameInstallations(SelectedGameInstallation?.GameInstallation);

    public record InstalledGameViewModel(
        GameInstallation GameInstallation, 
        GameDescriptor GameDescriptor, 
        LocalizedString DisplayName);
}