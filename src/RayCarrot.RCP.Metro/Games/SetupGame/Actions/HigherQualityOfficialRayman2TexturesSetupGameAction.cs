namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class HigherQualityOfficialRayman2TexturesSetupGameAction : InstallModSetupGameAction
{
    protected override long GameBananaModId => 479823;
    protected override string[] ModIds => ["Rayman2.Skins.OfficalHDTextures"];

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_HigherQualityOfficialRayman2Textures_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_HigherQualityOfficialRayman2Textures_Info));

    public override SetupGameActionType Type => SetupGameActionType.Optional;
}