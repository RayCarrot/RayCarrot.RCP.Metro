namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameManager_Rayman2_Win32 : SetupGameManager
{
    public SetupGameManager_Rayman2_Win32(GameInstallation gameInstallation) : base(gameInstallation) { }

    public override IEnumerable<SetupGameAction> GetRecommendedActions()
    {
        // TODO-UPDATE: Only add if GOG version
        // TODO-LOC
        // Ray2Fix
        yield return new SetupGameModAction(
            header: "Install Ray2Fix",
            info: "Ray2Fix is a mod, primarily for the GOG version, by spitfirex86 that aims to simplify setting up the game. It also comes bundled with various tweaks, such as the ability to remap gamepad controls and proper widescreen support.",
            isComplete: (GameInstallation.InstallLocation.Directory + "R2FixCfg.exe").FileExists,
            gameBananaModId: 479402);
    }
}