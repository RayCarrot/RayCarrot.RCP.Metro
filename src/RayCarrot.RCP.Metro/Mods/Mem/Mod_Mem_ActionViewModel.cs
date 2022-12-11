﻿using System.Windows.Input;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

public class Mod_Mem_ActionViewModel : BaseViewModel
{
    public Mod_Mem_ActionViewModel(LocalizedString header, PackIconMaterialKind iconKind, ICommand command, Func<bool>? isEnabledFunc)
    {
        Header = header;
        IconKind = iconKind;
        Command = command;
        IsEnabledFunc = isEnabledFunc;
    }

    protected Func<bool>? IsEnabledFunc { get; }

    public LocalizedString Header { get; }
    public PackIconMaterialKind IconKind { get; } 
    public ICommand Command { get; }
    public bool IsEnabled { get; set; }

    public void Refresh()
    {
        IsEnabled = IsEnabledFunc?.Invoke() ?? true;
    }
}