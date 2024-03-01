namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class GameViewModel : BaseViewModel
{
    public GameViewModel(Game game, LocalizedString displayName, Func<Dictionary<string, long>> getOffsetsFunc, EmulatorViewModel[] emulators, string[]? processNameKeywords = null)
    {
        Game = game;
        DisplayName = displayName;
        _getOffsetsFunc = getOffsetsFunc;
        Emulators = emulators;
        ProcessNameKeywords = processNameKeywords ?? Array.Empty<string>();
    }

    private readonly Func<Dictionary<string, long>> _getOffsetsFunc;

    public Game Game { get; }
    public LocalizedString DisplayName { get; }
    public EmulatorViewModel[] Emulators { get; }
    public string[] ProcessNameKeywords { get; }

    public Dictionary<string, long> GetOffsets() => _getOffsetsFunc();
}