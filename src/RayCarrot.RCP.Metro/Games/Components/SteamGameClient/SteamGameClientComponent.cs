namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// This component is to be used on games which can use <see cref="Clients.Steam.SteamGameClientDescriptor"/>
/// </summary>
[BaseGameComponent]
[SingleInstanceGameComponent]
public class SteamGameClientComponent : GameComponent
{
    public SteamGameClientComponent(string steamId)
    {
        SteamId = steamId;
    }

    public string SteamId { get; }
}