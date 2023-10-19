namespace RayCarrot.RCP.Metro.ModLoader.Modules;

public class ModModuleViewModel : BaseViewModel
{
    public ModModuleViewModel(ModModule module)
    {
        Module = module;
        Name = module.Id.ToUpperInvariant();
        Description = module.Description;
    }

    public ModModule Module { get; }
    public string Name { get; }
    public LocalizedString Description { get; }

    public bool IsEnabled { get; set; }
}