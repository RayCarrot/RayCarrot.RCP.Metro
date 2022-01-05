using System;

namespace RayCarrot.RCP.Metro;

public class Utility_SerializableTypeModeViewModel : BaseViewModel, IDisposable
{
    public Utility_SerializableTypeModeViewModel(LocalizedString name, Utility_SerializableTypeModeData data)
    {
        Name = name;
        Data = data;
    }

    public LocalizedString Name { get; }
    public Utility_SerializableTypeModeData Data { get; }

    public void Dispose()
    {
        Name.Dispose();
    }
}