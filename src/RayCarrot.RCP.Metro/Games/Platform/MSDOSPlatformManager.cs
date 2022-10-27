namespace RayCarrot.RCP.Metro;

public class MSDOSPlatformManager : EmulatedPlatformManager
{
    public MSDOSPlatformManager(MSDOSGameDescriptor gameDescriptor) : base(gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    public override GamePlatform Platform => GamePlatform.MSDOS;
    public new MSDOSGameDescriptor GameDescriptor { get; }

    public override Emulator[] Emulators => new Emulator[] { new Emulator_DOSBox() };
}