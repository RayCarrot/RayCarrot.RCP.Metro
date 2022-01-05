using System;

namespace RayCarrot.RCP.Metro;

public class Utility_Archives_ModeViewModel : BaseViewModel, IDisposable
{
    public Utility_Archives_ModeViewModel(LocalizedString name, Games? game, object data)
    {
        Name = name;
        Game = game;
        Data = data;
    }

    public LocalizedString Name { get; }
    public Games? Game { get; }
    public object Data { get; }

    public void Dispose()
    {
        Name.Dispose();
    }
}