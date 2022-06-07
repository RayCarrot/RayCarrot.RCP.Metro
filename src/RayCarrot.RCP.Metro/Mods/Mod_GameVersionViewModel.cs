using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

public class Mod_GameVersionViewModel : BaseViewModel
{
    public Mod_GameVersionViewModel(LocalizedString displayName, Func<Dictionary<string, long>> getOffsetsFunc)
    {
        DisplayName = displayName;
        GetOffsetsFunc = getOffsetsFunc;
    }

    public LocalizedString DisplayName { get; }
    public Func<Dictionary<string, long>> GetOffsetsFunc { get; }
}