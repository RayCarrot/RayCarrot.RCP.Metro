using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro.Games.Components;

public class GameOptionsDialogPageComponent : FactoryGameComponent<GameOptionsDialogPageViewModel>
{
    public GameOptionsDialogPageComponent(
        Func<GameInstallation, GameOptionsDialogPageViewModel> objFactory, 
        Func<GameInstallation, bool> isAvailableFunc, 
        PagePriority priority) 
        : base(objFactory)
    {
        _isAvailableFunc = isAvailableFunc;
        Priority = priority;
    }

    private readonly Func<GameInstallation, bool> _isAvailableFunc;

    public PagePriority Priority { get; }

    public bool IsAvailable(GameInstallation gameInstallation) => _isAvailableFunc(gameInstallation);
    
    public override GameOptionsDialogPageViewModel CreateObject(GameInstallation gameInstallation)
    {
        if (!IsAvailable(gameInstallation))
            throw new InvalidOperationException("The page can not be created when it is not available");

        return base.CreateObject(gameInstallation);
    }

    public enum PagePriority
    {
        Low = -1,
        Normal = 0,
        High = 1,
    }
}