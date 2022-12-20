namespace RayCarrot.RCP.Metro;

public class JumpListItemComparer : IComparer<JumpListItem>
{
    public JumpListItemComparer(GamesManager gamesManager)
    {
        GamesManager = gamesManager;
    }

    public GamesManager GamesManager { get; }

    public int Compare(JumpListItem? x, JumpListItem? y)
    {
        if (x == y)
            return 0;
        if (y == null || GamesManager.GetInstalledGame(y.GameInstallationId) is not { } yGame)
            return 1;
        if (x == null || GamesManager.GetInstalledGame(x.GameInstallationId) is not { } xGame)
            return -1;

        return xGame.CompareTo(yGame);
    }
}