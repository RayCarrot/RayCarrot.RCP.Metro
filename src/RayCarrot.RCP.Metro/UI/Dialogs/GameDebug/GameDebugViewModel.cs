using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

// TODO-14: Have a similar way of showing game client installation data?
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
    public ObservableCollection<GameComponentBuilder.Component>? Components { get; private set; }

    public ObservableCollection<GameIcon.GameIconSize> IconSizes { get; }

    private void Refresh()
    {
        GameDescriptor = SelectedGameInstallation?.GameDescriptor;
        IsDemo = GameDescriptor?.IsDemo ?? false;
        Icon = GameDescriptor?.Icon ?? GameIconAsset.Rayman1;

        if (SelectedGameInstallation != null && GameDescriptor != null)
        {
            // Get game data
            JsonSerializerSettings jsonSettings = new();
            GameInstallationJToken = JToken.FromObject(SelectedGameInstallation.GameInstallation, JsonSerializer.Create(jsonSettings));

            // Get components
            GameComponentBuilder builder = GameDescriptor.RegisterComponents(SelectedGameInstallation.GameInstallation);
            Components = builder.Build().OrderBy(x => x.BaseType.Name).ThenBy(x => x.InstanceType.Name).ToObservableCollection();
        }
        else
        {
            GameInstallationJToken = null;
            Components = null;
        }
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