using System;

namespace RayCarrot.RCP.Metro;

public class Serializers_TypeModeViewModel : BaseViewModel, IDisposable
{
    public Serializers_TypeModeViewModel(LocalizedString name, Serializers_TypeModeData data)
    {
        Name = name;
        Data = data;
    }

    public LocalizedString Name { get; }
    public Serializers_TypeModeData Data { get; }

    public void Dispose()
    {
        Name.Dispose();
    }
}