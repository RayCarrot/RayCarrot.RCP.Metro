namespace RayCarrot.RCP.Metro.Games.Tools.PrototypeRestoration;

public class MemoryPatch
{
    public MemoryPatch(int addressSteam, int addressGOG, byte[] bytes)
    {
        AddressSteam = addressSteam;
        AddressGOG = addressGOG;
        Bytes = bytes;
    }

    public int AddressSteam { get; }
    public int AddressGOG { get; }
    public byte[] Bytes { get; }

    public void Apply(int processHandle, bool isSteam)
    {
        MemoryManager.WriteProcessMemoryBytes(processHandle, isSteam ? AddressSteam : AddressGOG, Bytes);
    }
}