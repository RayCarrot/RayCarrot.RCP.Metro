namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// This component is to be used on games which can use <see cref="Clients.UbisoftConnect.UbisoftConnectGameClientDescriptor"/>
/// </summary>
[BaseGameComponent]
[SingleInstanceGameComponent]
public class UbisoftConnectGameClientComponent : GameComponent
{
    public UbisoftConnectGameClientComponent(string gameId)
    {
        GameId = gameId;
    }

    public string GameId { get; }
}