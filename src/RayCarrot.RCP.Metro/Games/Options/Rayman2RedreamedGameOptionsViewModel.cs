namespace RayCarrot.RCP.Metro.Games.Options;

/// <summary>
/// View model for Rayman 2: Redreamed game options
/// </summary>
public class Rayman2RedreamedGameOptionsViewModel : GameOptionsViewModel
{
    public Rayman2RedreamedGameOptionsViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        AvailableGraphicsApis =
        [
            new GraphicsApi(null, "Default"), // TODO-LOC
            new GraphicsApi("d3d11", "DirectX 11"),
            new GraphicsApi("d3d12", "DirectX 12"),
            new GraphicsApi("vulkan", "Vulkan")
        ];
    }

    public ObservableCollection<GraphicsApi> AvailableGraphicsApis { get; }

    public GraphicsApi SelectedGraphicsApi
    {
        get
        {
            string? id = GameInstallation.GetValue<string>(GameDataKey.R2R_GraphicsApi);
            return AvailableGraphicsApis.First(x => x.Id == id); 
        }
        set
        {
            GameInstallation.SetValue(GameDataKey.R2R_GraphicsApi, value.Id);
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }

    public class GraphicsApi : BaseViewModel
    {
        public GraphicsApi(string? id, LocalizedString displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public string? Id { get; }
        public LocalizedString DisplayName { get; }
    }
}