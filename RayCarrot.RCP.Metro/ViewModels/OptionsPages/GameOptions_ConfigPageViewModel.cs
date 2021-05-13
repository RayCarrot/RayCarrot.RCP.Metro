using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro
{
    public abstract class GameOptions_ConfigPageViewModel : GameOptions_BasePageViewModel
    {
        protected GameOptions_ConfigPageViewModel() : base(new LocalizedString(() => Resources.GameOptions_Config), PackIconMaterialKind.WrenchOutline) { }
    }
}