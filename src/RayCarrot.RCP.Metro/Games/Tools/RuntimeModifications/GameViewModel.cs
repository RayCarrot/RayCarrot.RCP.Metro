namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class GameViewModel : BaseViewModel
{
    public GameViewModel(GameManager gameManager)
    {
        GameManager = gameManager;
        DisplayName = gameManager.DisplayName;
    }

    public GameManager GameManager { get; }
    public LocalizedString DisplayName { get; }
}