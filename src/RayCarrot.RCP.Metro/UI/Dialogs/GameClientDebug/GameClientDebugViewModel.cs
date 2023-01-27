using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public class GameClientDebugViewModel : BaseViewModel, IInitializable,
    IRecipient<AddedGameClientsMessage>, IRecipient<RemovedGameClientsMessage>, IRecipient<ModifiedGameClientsMessage>
{
    public GameClientDebugViewModel(GameClientInstallation gameClientInstallation)
    {
        RefreshGameClientInstallations(gameClientInstallation);
    }

    private InstalledGameClientViewModel? _selectedGameClientInstallation;

    public ObservableCollection<InstalledGameClientViewModel>? GameClientInstallations { get; set; }

    public InstalledGameClientViewModel? SelectedGameClientInstallation
    {
        get => _selectedGameClientInstallation;
        set
        {
            _selectedGameClientInstallation = value;
            Refresh();
        }
    }

    public GameClientDescriptor? GameClientDescriptor { get; private set; }
    public GameClientIconAsset Icon { get; private set; }
    public JToken? GameClientDescriptorJToken { get; private set; }
    public JToken? GameClientInstallationJToken { get; private set; }

    private void Refresh()
    {
        GameClientDescriptor = SelectedGameClientInstallation?.GameClientDescriptor;
        Icon = GameClientDescriptor?.Icon ?? default;

        if (SelectedGameClientInstallation != null && GameClientDescriptor != null)
        {
            JsonSerializerSettings jsonSettings = new()
            {
                Converters = new JsonConverter[] { new StringEnumConverter() }
            };

            // Get game client descriptor
            GameClientDescriptorJToken = JToken.FromObject(GameClientDescriptor, JsonSerializer.Create(jsonSettings));

            // Get game client data
            GameClientInstallationJToken = JToken.FromObject(SelectedGameClientInstallation.GameClientInstallation, JsonSerializer.Create(jsonSettings));
        }
        else
        {
            GameClientDescriptorJToken = null;
            GameClientInstallationJToken = null;
        }
    }

    public void RefreshGameClientInstallations(GameClientInstallation? selectedGameInstallation)
    {
        GameClientInstallations = 
            new ObservableCollection<InstalledGameClientViewModel>(Services.GameClients.GetInstalledGameClients().
                Select(x => new InstalledGameClientViewModel(x, x.GameClientDescriptor, x.GetDisplayName())));

        SelectedGameClientInstallation = 
            GameClientInstallations.FirstOrDefault(x => x.GameClientInstallation == selectedGameInstallation) ?? 
            GameClientInstallations.FirstOrDefault();

        Refresh();
    }

    public void Initialize() => Services.Messenger.RegisterAll(this);
    public void Deinitialize() => Services.Messenger.UnregisterAll(this);

    public void Receive(AddedGameClientsMessage message) => RefreshGameClientInstallations(SelectedGameClientInstallation?.GameClientInstallation);
    public void Receive(RemovedGameClientsMessage message) => RefreshGameClientInstallations(SelectedGameClientInstallation?.GameClientInstallation);
    public void Receive(ModifiedGameClientsMessage message) => RefreshGameClientInstallations(SelectedGameClientInstallation?.GameClientInstallation);

    public record InstalledGameClientViewModel(
        GameClientInstallation GameClientInstallation,
        GameClientDescriptor GameClientDescriptor, 
        LocalizedString DisplayName);
}