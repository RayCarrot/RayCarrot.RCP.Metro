using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public class GameDebugViewModel : BaseViewModel, IInitializable,
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>
{
    public GameDebugViewModel(GameInstallation gameInstallation)
    {
        IconSizes = new ObservableCollection<GameIcon.GameIconSize>(EnumHelpers.GetValues<GameIcon.GameIconSize>());
        RefreshGameInstallations(gameInstallation);
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
    public JToken? GameDescriptorJToken { get; private set; }
    public JToken? GameInstallationJToken { get; private set; }
    public ObservableCollection<GameComponentBuilder.Component>? Components { get; private set; }

    public ObservableCollection<GameIcon.GameIconSize> IconSizes { get; }

    private void Refresh()
    {
        GameDescriptor = SelectedGameInstallation?.GameDescriptor;
        IsDemo = GameDescriptor?.IsDemo ?? false;
        Icon = GameDescriptor?.Icon ?? default;

        if (SelectedGameInstallation != null && GameDescriptor != null)
        {
            JsonSerializerSettings jsonSettings = new()
            {
                Converters = new JsonConverter[] { new StringEnumConverter() }
            };

            // Get game descriptor
            GameDescriptorJToken = JToken.FromObject(GameDescriptor, JsonSerializer.Create(jsonSettings));

            // Get game data
            GameInstallationJToken = JToken.FromObject(SelectedGameInstallation.GameInstallation, JsonSerializer.Create(jsonSettings));

            // Get components
            GameComponentBuilder builder = GameDescriptor.RegisterComponents(SelectedGameInstallation.GameInstallation);
            Components = builder.Build().OrderBy(x => x.BaseType.Name).ThenBy(x => x.InstanceType.Name).ToObservableCollection();
        }
        else
        {
            GameDescriptorJToken = null;
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

    public void Initialize() => Services.Messenger.RegisterAll(this);
    public void Deinitialize() => Services.Messenger.UnregisterAll(this);

    void IRecipient<AddedGamesMessage>.Receive(AddedGamesMessage message) => RefreshGameInstallations(SelectedGameInstallation?.GameInstallation);
    void IRecipient<RemovedGamesMessage>.Receive(RemovedGamesMessage message) => RefreshGameInstallations(SelectedGameInstallation?.GameInstallation);
    void IRecipient<ModifiedGamesMessage>.Receive(ModifiedGamesMessage message) => RefreshGameInstallations(SelectedGameInstallation?.GameInstallation);

    public record InstalledGameViewModel(
        GameInstallation GameInstallation, 
        GameDescriptor GameDescriptor, 
        LocalizedString DisplayName);
}