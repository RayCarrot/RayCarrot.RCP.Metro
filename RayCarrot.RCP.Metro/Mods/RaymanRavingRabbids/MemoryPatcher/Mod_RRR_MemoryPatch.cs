namespace RayCarrot.RCP.Metro
{
    public class Mod_RRR_MemoryPatch
    {
        public Mod_RRR_MemoryPatch(int addressSteam, int addressGOG, byte[] bytes)
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
            Mod_RRR_MemoryManager.WriteProcessMemoryBytes(processHandle, isSteam ? AddressSteam : AddressGOG, Bytes);
        }
    }
}