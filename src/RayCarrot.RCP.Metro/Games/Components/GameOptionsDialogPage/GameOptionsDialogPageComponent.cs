using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class GameOptionsDialogPageComponent : FactoryGameComponent<GameOptionsDialogPageViewModel>
{
    public GameOptionsDialogPageComponent(
        Func<GameInstallation, GameOptionsDialogPageViewModel> objFactory, 
        Func<GameInstallation, bool> isAvailableFunc, 
        Func<GameInstallation, string> getInstanceIdFunc) 
        : base(objFactory)
    {
        _isAvailableFunc = isAvailableFunc;
        _getInstanceIdFunc = getInstanceIdFunc;
    }

    private readonly Func<GameInstallation, bool> _isAvailableFunc;
    private readonly Func<GameInstallation, string> _getInstanceIdFunc;

    public bool IsAvailable() => _isAvailableFunc(GameInstallation);
    public string GetInstanceId() => _getInstanceIdFunc(GameInstallation);
    
    public override GameOptionsDialogPageViewModel CreateObject()
    {
        if (!IsAvailable())
            throw new InvalidOperationException("The page can not be created when it is not available");

        return base.CreateObject();
    }
}