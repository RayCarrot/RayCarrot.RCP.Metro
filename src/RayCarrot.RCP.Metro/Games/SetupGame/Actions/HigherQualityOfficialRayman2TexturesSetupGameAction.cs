namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class HigherQualityOfficialRayman2TexturesSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 479823;
    protected override string ModId => "Rayman2.Skins.OfficalHDTextures";

    // TODO-LOC
    public override LocalizedString Header => "Install the Higher Quality Official Textures mod";
    public override LocalizedString Info => "The Higher Quality Official Textures mod is a texture mod by ICUP321 which replaces many of the textures with better/higher quality variants found on other versions, including textures with transparency fixes.";

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}