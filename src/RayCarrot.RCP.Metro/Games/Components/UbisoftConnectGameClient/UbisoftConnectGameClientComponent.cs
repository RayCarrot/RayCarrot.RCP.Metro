namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// This component is to be used on games which can use <see cref="Clients.UbisoftConnect.UbisoftConnectGameClientDescriptor"/>
/// </summary>
[GameComponentBase(SingleInstance = true)]
public class UbisoftConnectGameClientComponent : GameComponent
{
    public UbisoftConnectGameClientComponent(string gameId, string productId)
    {
        GameId = gameId;
        ProductId = productId;
    }

    public string GameId { get; }
    public string ProductId { get; }
}