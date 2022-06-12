using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

public class Mod_Mem_GameViewModel : BaseViewModel
{
    public Mod_Mem_GameViewModel(Mod_Mem_Game game, LocalizedString displayName, Func<Dictionary<string, long>> getOffsetsFunc, Mod_Mem_EmulatorViewModel[] emulators, string[]? processNameKeywords = null)
    {
        Game = game;
        DisplayName = displayName;
        _getOffsetsFunc = getOffsetsFunc;
        Emulators = emulators;
        ProcessNameKeywords = processNameKeywords ?? Array.Empty<string>();
    }

    private readonly Func<Dictionary<string, long>> _getOffsetsFunc;

    public Mod_Mem_Game Game { get; }
    public LocalizedString DisplayName { get; }
    public Mod_Mem_EmulatorViewModel[] Emulators { get; }
    public string[] ProcessNameKeywords { get; }

    public Dictionary<string, long> GetOffsets() => _getOffsetsFunc();
}