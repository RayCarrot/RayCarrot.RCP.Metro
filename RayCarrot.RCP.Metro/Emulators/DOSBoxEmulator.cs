namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The DOSBox emulator for emulating MS-DOS games
    /// </summary>
    public class DOSBoxEmulator : Emulator
    {
        /// <summary>
        /// The display name of the emulator
        /// </summary>
        public override LocalizedString DisplayName => new LocalizedString(() => Resources.GameType_DosBox);

        /// <summary>
        /// The emulator's game configuration UI
        /// </summary>
        public override object GameConfigUI => "Test"; // TODO: Move DOSBox config over here
    }
}