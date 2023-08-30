namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class GameBananaGameComponent : GameComponent
{
    public GameBananaGameComponent(int gameId)
    {
        GameId = gameId;
    }

    public int GameId { get; }
}