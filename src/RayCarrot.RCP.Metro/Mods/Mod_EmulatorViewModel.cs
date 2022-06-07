namespace RayCarrot.RCP.Metro;

public class Mod_EmulatorViewModel : BaseViewModel
{
    public Mod_EmulatorViewModel(LocalizedString displayName, string[] processNameKeywords, long gameBaseOffset, bool isGameBaseAPointer)
    {
        DisplayName = displayName;
        ProcessNameKeywords = processNameKeywords;
        GameBaseOffset = gameBaseOffset;
        IsGameBaseAPointer = isGameBaseAPointer;
    }

    public LocalizedString DisplayName { get; }
    public string[] ProcessNameKeywords { get; }
    public long GameBaseOffset { get; }
    public bool IsGameBaseAPointer { get; }

    // TODO-UPDATE: Localize
    public static Mod_EmulatorViewModel DOSBox_0_74 => new(
        displayName: "DOSBox 0.74",
        processNameKeywords: new[] { "DOSBox" },
        gameBaseOffset: 0x01D3A1A0,
        isGameBaseAPointer: true);
}