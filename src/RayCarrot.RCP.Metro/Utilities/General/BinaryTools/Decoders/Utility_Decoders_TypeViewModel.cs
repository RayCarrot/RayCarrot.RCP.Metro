using System;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class Utility_Decoders_TypeViewModel : BaseRCPViewModel, IDisposable
{
    public Utility_Decoders_TypeViewModel(LocalizedString name, IStreamEncoder encoder, Func<string> getFileFilter, Games? game = null)
    {
        Name = name;
        Encoder = encoder;
        GetFileFilter = getFileFilter;
        Game = game;
    }

    public LocalizedString Name { get; }
    public IStreamEncoder Encoder { get; }
    public Func<string> GetFileFilter { get; }
    public Games? Game { get; }

    public void Dispose()
    {
        Name.Dispose();
    }
}