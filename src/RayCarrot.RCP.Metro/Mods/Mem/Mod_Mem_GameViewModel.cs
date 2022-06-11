using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

public class Mod_Mem_GameViewModel : BaseViewModel
{
    public Mod_Mem_GameViewModel(Mod_Mem_Game game, LocalizedString displayName, Func<Dictionary<string, long>> getOffsetsFunc, Mod_Mem_EmulatorViewModel[] emulators)
    {
        Game = game;
        DisplayName = displayName;
        Emulators = emulators;
        _getOffsetsFunc = getOffsetsFunc;
    }

    private readonly Func<Dictionary<string, long>> _getOffsetsFunc;

    public Mod_Mem_Game Game { get; }
    public LocalizedString DisplayName { get; }
    public Mod_Mem_EmulatorViewModel[] Emulators { get; }

    public Dictionary<string, long> GetOffsets() => _getOffsetsFunc();
}