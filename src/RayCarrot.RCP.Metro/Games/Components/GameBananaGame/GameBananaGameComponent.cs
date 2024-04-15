namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class GameBananaGameComponent : GameComponent
{
    public GameBananaGameComponent(int gameId)
    {
        GameId = gameId;
    }

    public int GameId { get; }
}