using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

public class Mod_GameVersionViewModel<T> : BaseViewModel
{
    public Mod_GameVersionViewModel(LocalizedString displayName, Func<Dictionary<string, long>> getOffsetsFunc, T data)
    {
        DisplayName = displayName;
        GetOffsetsFunc = getOffsetsFunc;
        Data = data;
    }

    public LocalizedString DisplayName { get; }
    public Func<Dictionary<string, long>> GetOffsetsFunc { get; }
    public T Data { get; }
}