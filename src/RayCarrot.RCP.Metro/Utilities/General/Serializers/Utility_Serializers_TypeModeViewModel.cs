using System;

namespace RayCarrot.RCP.Metro;

public class Utility_Serializers_TypeModeViewModel : BaseViewModel, IDisposable
{
    public Utility_Serializers_TypeModeViewModel(LocalizedString name, Utility_Serializers_TypeModeData data)
    {
        Name = name;
        Data = data;
    }

    public LocalizedString Name { get; }
    public Utility_Serializers_TypeModeData Data { get; }

    public void Dispose()
    {
        Name.Dispose();
    }
}